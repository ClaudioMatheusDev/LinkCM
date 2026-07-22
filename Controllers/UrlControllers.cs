using LinkCM.Data;
using LinkCM.DTOs;
using LinkCM.Models;
using LinkCM.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinkCM.Controllers
{
    [ApiController]
    [Route("api/urls")]
    public class UrlControllers : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly GeradorCodigosCurtos _codeGenerator;

        public UrlControllers(AppDbContext context, GeradorCodigosCurtos codeGenerator)
        {
            _context = context;
            _codeGenerator = codeGenerator;
        }

        [HttpPost]
        public async Task<ActionResult<RespostaURLCurta>> Create(URLCurtaRequisicao request)
        {
            if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return BadRequest("Informe uma URL valida com http ou https.");
            }

            var shortCode = request.CustomCode;

            if (!string.IsNullOrWhiteSpace(shortCode))
            {
                var customCodeExists = await _context.ShortUrls
                    .AnyAsync(url => url.ShortCode == shortCode);

                if (customCodeExists)
                {
                    return Conflict("Esse codigo curto ja esta em uso.");
                }
            }
            else
            {
                do
                {
                    shortCode = _codeGenerator.GerarCodigoCurto();
                }
                while (await _context.ShortUrls.AnyAsync(url => url.ShortCode == shortCode));
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var urlCurta = new URLCurta
            {
                UrlOriginal = request.Url,
                ShortCode = shortCode!,
                UrlOtimizada = $"{baseUrl}/{shortCode}",
                DataCriacao = DateTime.Now,
                DataExpira = request.DataExpira ?? DateTime.Now.AddYears(1),
                QuantidadeCliques = 0,
                Ativo = true
            };

            _context.ShortUrls.Add(urlCurta);
            await _context.SaveChangesAsync();

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

            return CreatedAtAction(nameof(Create), new { id = urlCurta.Id }, response);
        }
    }
}
