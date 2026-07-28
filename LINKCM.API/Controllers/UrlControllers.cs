using LinkCM.Data;
using LinkCM.DTOs;
using LinkCM.Models;
using LinkCM.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LinkCM.Controllers
{
    [ApiController]
    [Route("api/urls")]
    public class UrlControllers : ControllerBase
    {
        private const int MaximoTentativasCodigoAutomatico = 5;

        private readonly AppDbContext _context;
        private readonly GeradorCodigosCurtos _codeGenerator;
        private readonly ILogger<UrlControllers> _logger;

        public UrlControllers(
            AppDbContext context,
            GeradorCodigosCurtos codeGenerator,
            ILogger<UrlControllers> logger)
        {
            _context = context;
            _codeGenerator = codeGenerator;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<RespostaURLCurta>> Create(
            URLCurtaRequisicao request,
            CancellationToken cancellationToken)
        {
            var DataHoraAgora = DateTime.UtcNow.AddHours(-3);

            if (!UrlValida(request.Url))
            {
                return BadRequest(
                    "Informe uma URL válida começando com http ou https.");
            }

            if (request.DataExpira.HasValue &&
                request.DataExpira.Value <= DataHoraAgora)
            {
                return BadRequest(
                    "A data de expiração deve ser uma data futura.");
            }

            var customCode = request.CustomCode?.Trim();

            if (!string.IsNullOrWhiteSpace(customCode))
            {
                return await CriarComCodigoPersonalizado(
                    request,
                    customCode,
                    cancellationToken);
            }

            return await CriarComCodigoAutomatico(
                request,
                cancellationToken);
        }

        private async Task<ActionResult<RespostaURLCurta>>
            CriarComCodigoPersonalizado(
                URLCurtaRequisicao request,
                string customCode,
                CancellationToken cancellationToken)
        {
            if (!GeradorCodigosCurtos.CodigoPersonalizadoValido(customCode))
            {
                return BadRequest(
                    "O código curto personalizado deve ter exatamente " +
                    "6 caracteres alfanuméricos.");
            }

            // Pré-checagem para retornar 409 mais cedo.
            // Não é a garantia definitiva contra concorrência.
            var customCodeExists = await _context.ShortUrls
                .AnyAsync(
                    url => url.ShortCode == customCode,
                    cancellationToken);

            if (customCodeExists)
            {
                return Conflict("Esse código curto já está em uso.");
            }

            var urlCurta = CriarEntidade(request, customCode);

            _context.ShortUrls.Add(urlCurta);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);

                return CriarRespostaCreated(urlCurta);
            }
            catch (DbUpdateException exception)
                when (EhViolacaoDoIndiceShortCode(exception))
            {
                _context.Entry(urlCurta).State = EntityState.Detached;

                return Conflict("Esse código curto já está em uso.");
            }
        }

        private async Task<ActionResult<RespostaURLCurta>>
            CriarComCodigoAutomatico(
                URLCurtaRequisicao request,
                CancellationToken cancellationToken)
        {
            for (var tentativa = 1;
                 tentativa <= MaximoTentativasCodigoAutomatico;
                 tentativa++)
            {
                var shortCode = _codeGenerator.GerarCodigoCurto();

                var urlCurta = CriarEntidade(request, shortCode);

                _context.ShortUrls.Add(urlCurta);

                try
                {
                    await _context.SaveChangesAsync(cancellationToken);

                    return CriarRespostaCreated(urlCurta);
                }
                catch (DbUpdateException exception)
                    when (EhViolacaoDoIndiceShortCode(exception))
                {
                    _context.Entry(urlCurta).State = EntityState.Detached;

                    _logger.LogWarning(
                        "Colisão ao gerar ShortCode {ShortCode}. " +
                        "Tentativa {Tentativa} de {MaximoTentativas}.",
                        shortCode,
                        tentativa,
                        MaximoTentativasCodigoAutomatico);
                }
            }

            _logger.LogError(
                "Não foi possível gerar um ShortCode único após " +
                "{MaximoTentativas} tentativas.",
                MaximoTentativasCodigoAutomatico);

            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                "Não foi possível gerar uma URL curta no momento. " +
                "Tente novamente.");
        }

        private URLCurta CriarEntidade(
            URLCurtaRequisicao request,
            string shortCode)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var DataHoraAgora = DateTime.UtcNow.AddHours(-3);

            var dataExpiraFinal =
                request.DataExpira ?? DataHoraAgora.AddYears(1);

            return new URLCurta
            {
                UrlOriginal = request.Url,
                ShortCode = shortCode,
                UrlOtimizada = $"{baseUrl}/{shortCode}",
                DataCriacao = DataHoraAgora,
                DataExpira = dataExpiraFinal,
                QuantidadeCliques = 0,
                Ativo = true
            };
        }
        private ActionResult<RespostaURLCurta> CriarRespostaCreated(
            URLCurta urlCurta)
        {
            var response = new RespostaURLCurta
            {
                Id = urlCurta.Id,
                UrlCurta = urlCurta.UrlOtimizada,
                UrlOriginal = urlCurta.UrlOriginal,
                ShortCode = urlCurta.ShortCode,
                DataCriacao = urlCurta.DataCriacao,
                DataExpira = urlCurta.DataExpira,
                ContasAcessadas = urlCurta.QuantidadeCliques
            };

            return Created(urlCurta.UrlOtimizada, response);
        }

        private static bool UrlValida(string url)
        {
            return Uri.TryCreate(
                       url,
                       UriKind.Absolute,
                       out var uri) &&
                   (uri.Scheme == Uri.UriSchemeHttp ||
                    uri.Scheme == Uri.UriSchemeHttps);
        }

        private static bool EhViolacaoDoIndiceShortCode(
            DbUpdateException exception)
        {
            var sqlException =
                exception.InnerException as SqlException ??
                exception.GetBaseException() as SqlException;

            if (sqlException is null)
            {
                return false;
            }

            var ehDuplicidade =
                sqlException.Number is 2601 or 2627;

            if (!ehDuplicidade)
            {
                return false;
            }

            return sqlException.Message.Contains(
                "IX_ShortUrls_ShortCode",
                StringComparison.OrdinalIgnoreCase);
        }

        [HttpGet("/{shortCode}")]
        public async Task<ActionResult> RedirectToOriginal(
            string shortCode,
            CancellationToken cancellationToken)
        {   
            var DataHoraAgora = DateTime.UtcNow.AddHours(-3);
            var urlCurta = await _context.ShortUrls
                .FirstOrDefaultAsync(
                    url => url.ShortCode == shortCode,
                    cancellationToken);

            if (urlCurta is null ||
                !urlCurta.Ativo ||
                urlCurta.DataExpira <= DataHoraAgora)
            {
                return NotFound(
                    "URL curta não encontrada ou expirada.");
            }

            urlCurta.QuantidadeCliques++;
            urlCurta.UltimoAcesso = DataHoraAgora;

            await _context.SaveChangesAsync(cancellationToken);

            return Redirect(urlCurta.UrlOriginal);
        }
    }
}