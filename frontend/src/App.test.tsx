import '@testing-library/jest-dom/vitest'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, expect, it, vi } from 'vitest'
import { App } from './App'
import { request } from './api'

vi.mock('./api', () => ({ getBoard: () => 'test-board', request: vi.fn() }))

afterEach(() => {
  cleanup()
  vi.resetAllMocks()
})

it('carga tareas y solicita el filtro de pendientes', async () => {
  const call = vi.mocked(request)
  call.mockResolvedValueOnce({
    items: [{ id: '1', title: 'Conciliar cuentas', completed: true }],
    total: 1, pending: 0, completed: 1,
  }).mockResolvedValueOnce({ items: [], total: 1, pending: 0, completed: 1 })
  render(<App />)

  expect(await screen.findByRole('checkbox', { name: 'Conciliar cuentas' })).toBeChecked()
  fireEvent.click(screen.getByRole('button', { name: 'Pendientes' }))
  expect(await screen.findByText('Sin tareas pendientes')).toBeVisible()
  expect(call).toHaveBeenLastCalledWith('?completed=false', 'test-board', expect.any(Object))
})

it('envía el título al servidor y muestra el resultado actualizado', async () => {
  const call = vi.mocked(request)
  call.mockResolvedValueOnce({ items: [], total: 0, pending: 0, completed: 0 })
    .mockResolvedValueOnce({ id: '1', title: 'Registrar factura', completed: false })
    .mockResolvedValueOnce({
      items: [{ id: '1', title: 'Registrar factura', completed: false }],
      total: 1, pending: 1, completed: 0,
    })
  render(<App />)

  const input = screen.getByRole('textbox', { name: 'Nueva tarea' })
  fireEvent.change(input, { target: { value: 'Registrar factura' } })
  fireEvent.click(screen.getByRole('button', { name: 'Agregar' }))
  expect(await screen.findByRole('checkbox', { name: 'Registrar factura' })).toBeVisible()
  expect(call).toHaveBeenCalledWith('', 'test-board', {
    method: 'POST', body: JSON.stringify({ title: 'Registrar factura' }),
  })
  expect(input).toHaveValue('')
})

it('permite recuperar la lista después de un error', async () => {
  const call = vi.mocked(request)
  call.mockRejectedValueOnce(new Error('No se pudo conectar'))
    .mockResolvedValueOnce({ items: [], total: 0, pending: 0, completed: 0 })
  render(<App />)
  expect(await screen.findByRole('alert')).toHaveTextContent('No se pudo conectar')
  fireEvent.click(screen.getByRole('button', { name: 'Reintentar' }))
  await waitFor(() => expect(screen.queryByRole('alert')).not.toBeInTheDocument())
  expect(await screen.findByText('Agrega una tarea para empezar.')).toBeVisible()
})
