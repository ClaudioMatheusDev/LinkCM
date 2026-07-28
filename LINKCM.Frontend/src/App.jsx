import { useState } from 'react'
import './App.css'

const apiUrl = import.meta.env.VITE_API_URL

function App() {
  const [url, setUrl] = useState('')
  const [shortUrl, setShortUrl] = useState('')
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [isCopied, setIsCopied] = useState(false)

  async function handleSubmit(event) {
    event.preventDefault()
    setError('')
    setShortUrl('')
    setIsCopied(false)

    if (!url.trim()) {
      setError('Informe uma URL para encurtar.')
      return
    }

    setIsLoading(true)

    try {
      const response = await fetch(`${apiUrl}/api/urls`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ url: url.trim() }),
      })

      if (!response.ok) {
        const message = await response.text()
        throw new Error(message || 'Não foi possível encurtar esta URL.')
      }

      const data = await response.json()
      setShortUrl(data.urlCurta)
    } catch (requestError) {
      setError(requestError.message || 'Não foi possível conectar à API.')
    } finally {
      setIsLoading(false)
    }
  }

  async function copyShortUrl() {
    try {
      await navigator.clipboard.writeText(shortUrl)
      setIsCopied(true)
    } catch {
      setError('Não foi possível copiar o link. Copie-o manualmente.')
    }
  }

  return (
    <main className="page">
      <section className="card" aria-labelledby="page-title">
        <p className="brand">LinkCM</p>
        <h1 id="page-title">Encurte seu link</h1>
        <p className="description">Cole uma URL longa e gere um link mais simples para compartilhar.</p>

        <form onSubmit={handleSubmit}>
          <label htmlFor="url">URL original</label>
          <div className="form-row">
            <input
              id="url"
              name="url"
              type="url"
              value={url}
              onChange={(event) => setUrl(event.target.value)}
              placeholder="https://exemplo.com/minha-url-muito-longa"
              autoComplete="url"
              required
            />
            <button type="submit" disabled={isLoading}>
              {isLoading ? 'Gerando...' : 'Encurtar'}
            </button>
          </div>
        </form>

        {error && <p className="message error" role="alert">{error}</p>}

        {shortUrl && (
          <div className="result" aria-live="polite">
            <span>Seu link encurtado</span>
            <div className="result-row">
              <a href={shortUrl} target="_blank" rel="noreferrer">{shortUrl}</a>
              <button type="button" className="copy-button" onClick={copyShortUrl}>
                {isCopied ? 'Copiado!' : 'Copiar'}
              </button>
            </div>
          </div>
        )}
      </section>
    </main>
  )
}

export default App
