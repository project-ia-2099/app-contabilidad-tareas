export type Task = { id: string; title: string; completed: boolean }
export type TaskList = { items: Task[]; total: number; pending: number; completed: number }

const storageKey = 'contabilidad-tareas-board'

export function getBoard() {
  const existing = localStorage.getItem(storageKey)
  if (existing) return existing

  const board = crypto.randomUUID()
  localStorage.setItem(storageKey, board)
  return board
}

export async function request<T>(path: string, board: string, init: RequestInit = {}): Promise<T> {
  const base = import.meta.env.VITE_API_URL as string | undefined
  if (!base) throw new Error('Falta configurar la dirección de la API.')

  const headers = new Headers(init.headers)
  headers.set('X-Task-Board', board)
  if (init.body) headers.set('Content-Type', 'application/json')

  let response: Response
  try {
    response = await fetch(`${base.replace(/\/$/, '')}/api/tasks${path}`, { ...init, headers })
  } catch (error) {
    if (init.signal?.aborted) throw error
    throw new Error('No pudimos conectar con el servidor. Vuelve a intentarlo.')
  }
  if (!response.ok) {
    const problem = response.headers.get('Content-Type')?.includes('json') ? await response.json() : null
    throw new Error(problem?.detail ?? 'No pudimos procesar la solicitud. Actualiza la lista e inténtalo de nuevo.')
  }
  return response.status === 204 ? undefined as T : await response.json() as T
}
