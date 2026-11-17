import { useEffect, useRef, useState, type FormEvent } from 'react'
import { getBoard, request, type Task, type TaskList } from './api'

const filters = [
  { key: 'all', label: 'Todas', query: '' },
  { key: 'pending', label: 'Pendientes', query: '?completed=false' },
  { key: 'completed', label: 'Completadas', query: '?completed=true' },
] as const
type Filter = typeof filters[number]

const emptyMessages = {
  all: 'Agrega una tarea para empezar.',
  pending: 'Estás al día con tus tareas.',
  completed: 'Las tareas que completes aparecerán aquí.',
}

export function App() {
  const [filter, setFilter] = useState<Filter>(filters[0])
  const [result, setResult] = useState<{ filter: Filter; data: TaskList }>()
  const [revision, setRevision] = useState(0)
  const [loading, setLoading] = useState(true)
  const [loadError, setLoadError] = useState('')
  const [actionError, setActionError] = useState('')
  const [notice, setNotice] = useState('')
  const [title, setTitle] = useState('')
  const [editing, setEditing] = useState<{ id: string; title: string }>()
  const [saving, setSaving] = useState(false)
  const [deleting, setDeleting] = useState<Task>()
  const confirmation = useRef<HTMLDialogElement>(null)

  useEffect(() => {
    const controller = new AbortController()
    setLoading(true)
    setLoadError('')

    async function load() {
      try {
        const data = await request<TaskList>(filter.query, getBoard(), { signal: controller.signal })
        if (!controller.signal.aborted) setResult({ filter, data })
      } catch (error) {
        if (!controller.signal.aborted) setLoadError(errorMessage(error))
      } finally {
        if (!controller.signal.aborted) setLoading(false)
      }
    }

    void load()
    return () => controller.abort()
  }, [filter, revision])

  async function mutate(path: string, init: RequestInit, onSuccess: () => void, message: string) {
    setSaving(true)
    setActionError('')
    setNotice('')
    try {
      await request(path, getBoard(), init)
      onSuccess()
      setRevision(value => value + 1)
      setNotice(message)
    } catch (error) {
      setActionError(errorMessage(error))
    } finally {
      setSaving(false)
    }
  }

  function add(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    void mutate('', { method: 'POST', body: JSON.stringify({ title }) }, () => {
      setTitle('')
      if (filter.key === 'completed') setFilter(filters[0])
    }, 'Tarea agregada.')
  }

  function rename(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!editing) return
    void mutate(`/${editing.id}`, {
      method: 'PATCH',
      body: JSON.stringify({ title: editing.title }),
    }, () => setEditing(undefined), 'Tarea actualizada.')
  }

  function toggle(task: Task) {
    void mutate(`/${task.id}`, {
      method: 'PATCH',
      body: JSON.stringify({ completed: !task.completed }),
    }, () => {}, task.completed ? 'Tarea reabierta.' : 'Tarea completada.')
  }

  function askToRemove(task: Task) {
    setDeleting(task)
    confirmation.current?.showModal()
  }

  function remove() {
    if (!deleting) return
    confirmation.current?.close()
    void mutate(`/${deleting.id}`, { method: 'DELETE' }, () => {}, 'Tarea eliminada.')
  }

  const list = result?.filter === filter ? result.data : undefined

  return (
    <>
      <header className="topbar">
        <div className="topbar-inner">
          <span className="brand-icon"><i className="fa-solid fa-list-check" aria-hidden="true" /></span>
          <span>Contabilidad</span>
        </div>
      </header>
      <main>
        <h1>Mis tareas</h1>
        <p className="subtitle">Organiza los pendientes de tu jornada.</p>

        <section className="summary" aria-label="Resumen de tareas">
          <Metric label="Totales" value={result?.data.total} icon="fa-layer-group" tone="total" />
          <Metric label="Pendientes" value={result?.data.pending} icon="fa-clock" tone="pending" />
          <Metric label="Completadas" value={result?.data.completed} icon="fa-circle-check" tone="done" />
        </section>

        <section className="panel" aria-labelledby="list-heading">
          <h2 id="list-heading">Lista de tareas</h2>
          <form className="composer" onSubmit={add}>
            <input aria-label="Nueva tarea" placeholder="¿Qué tienes pendiente?" value={title}
              required disabled={saving} onChange={event => setTitle(event.target.value)} />
            <button className="primary" type="submit" disabled={saving}>
              <i className="fa-solid fa-plus" aria-hidden="true" /> Agregar
            </button>
          </form>

          <div className="filters" role="group" aria-label="Filtrar tareas">
            {filters.map(option => (
              <button type="button" key={option.key} aria-pressed={filter === option}
                onClick={() => { setFilter(option); setEditing(undefined) }}>
                {option.label}
              </button>
            ))}
          </div>

          <div className="sr-only" role="status">{notice}</div>
          {actionError && <p className="error" role="alert">{actionError}</p>}
          {loadError && (
            <div className="error" role="alert">
              {loadError} <button type="button" onClick={() => setRevision(value => value + 1)}>Reintentar</button>
            </div>
          )}
          {loading && !list && <p className="empty" role="status">Cargando tareas…</p>}
          {list?.items.length === 0 && (
            <div className="empty">
              <span className="empty-icon"><i className="fa-solid fa-clipboard-list" aria-hidden="true" /></span>
              <strong>Sin tareas {filter.key === 'all' ? '' : filter.label.toLowerCase()}</strong>
              <p>{emptyMessages[filter.key]}</p>
            </div>
          )}

          {list && list.items.length > 0 && (
            <ul className="tasks" aria-busy={loading}>
              {list.items.map(task => (
                <li className={task.completed ? 'task done-task' : 'task'} key={task.id}>
                  {editing?.id === task.id ? (
                    <form className="edit-form" onSubmit={rename}>
                      <input aria-label="Título de la tarea" value={editing.title} required autoFocus
                        disabled={saving} onChange={event => setEditing({ id: task.id, title: event.target.value })} />
                      <button className="primary" type="submit" disabled={saving}>Guardar</button>
                      <button className="neutral" type="button" disabled={saving}
                        onClick={() => setEditing(undefined)}>Cancelar</button>
                    </form>
                  ) : (
                    <>
                      <label className="task-label">
                        <input className="task-checkbox" type="checkbox" checked={task.completed}
                          disabled={saving || loading} onChange={() => toggle(task)} />
                        <span>{task.title}</span>
                      </label>
                      <button className="icon-button" type="button" aria-label={`Editar ${task.title}`} title="Editar"
                        disabled={saving || loading} onClick={() => setEditing({ id: task.id, title: task.title })}>
                        <i className="fa-solid fa-pen" aria-hidden="true" />
                      </button>
                      <button className="icon-button delete" type="button" aria-label={`Eliminar ${task.title}`} title="Eliminar"
                        disabled={saving || loading} onClick={() => askToRemove(task)}>
                        <i className="fa-solid fa-trash" aria-hidden="true" />
                      </button>
                    </>
                  )}
                </li>
              ))}
            </ul>
          )}
        </section>
      </main>
      <dialog ref={confirmation} className="confirm" aria-labelledby="confirm-title" onClose={() => setDeleting(undefined)}>
        <h2 id="confirm-title">¿Eliminar esta tarea?</h2>
        <p>{deleting?.title}</p>
        <div className="confirm-actions">
          <button className="neutral" type="button" onClick={() => confirmation.current?.close()}>Cancelar</button>
          <button className="danger" type="button" onClick={remove}>Eliminar tarea</button>
        </div>
      </dialog>
    </>
  )
}

function Metric({ label, value, icon, tone }: { label: string; value?: number; icon: string; tone: string }) {
  return (
    <div className="metric">
      <span className={`metric-icon ${tone}`}><i className={`fa-solid ${icon}`} aria-hidden="true" /></span>
      <div><span>{label}</span><strong>{value ?? '–'}</strong></div>
    </div>
  )
}

function errorMessage(error: unknown) {
  return error instanceof Error ? error.message : 'Ocurrió un error inesperado.'
}
