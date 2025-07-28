"use client"
import { useState } from "react"

export default function DebugPage() {
  const [attachmentId, setAttachmentId] = useState("")
  const [imageUrl, setImageUrl] = useState("")

  const handleShow = async () => {
    const backendBaseUrl = "https://localhost:7205"
    const url = `${backendBaseUrl}/api/attachments/${attachmentId}/download`
    try {
      const response = await fetch(url, {
        credentials: "include", // ważne, jeśli JWT jest w cookie
      })
      if (!response.ok) {
        alert("Błąd pobierania pliku")
        return
      }
      const blob = await response.blob()
      setImageUrl(URL.createObjectURL(blob))
    } catch (err) {
      alert("Błąd połączenia z serwerem")
    }
  }

  return (
    <div className="p-4">
      <h1 className="text-xl font-bold mb-4">Test Wyświetlania Załącznika</h1>
      <input
        type="text"
        placeholder="ID załącznika"
        value={attachmentId}
        onChange={(e) => setAttachmentId(e.target.value)}
        className="border p-2"
      />
      <button onClick={handleShow} className="ml-2 p-2 bg-blue-500 text-white">
        Pokaż
      </button>
      {imageUrl && (
        <div className="mt-4">
          <img
            src={imageUrl}
            alt="Załącznik"
            className="max-w-sm border"
            crossOrigin="anonymous"
          />
        </div>
      )}
    </div>
  )
}
