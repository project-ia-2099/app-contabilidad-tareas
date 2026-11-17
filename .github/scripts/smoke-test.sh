#!/usr/bin/env bash
set -euo pipefail

api=${API_URL:?API_URL es requerida}
web=${WEB_URL:?WEB_URL es requerida}
board=$(cat /proc/sys/kernel/random/uuid)
header="X-Task-Board: $board"
path="$api/api/tasks"
id=''

cleanup() {
  if [[ -n "$id" ]]; then
    curl -fsS --max-time 15 -X DELETE -H "$header" "$path/$id" >/dev/null ||
      echo 'No se pudo limpiar la tarea temporal.' >&2
  fi
}
trap cleanup EXIT

curl -fsS --max-time 30 --retry 3 --retry-all-errors "$api/health" | jq -e '.status == "ok"' >/dev/null
page=$(curl -fsS --max-time 30 --retry 3 --retry-all-errors "$web/")
asset=$(grep -oE '/assets/[^"]+\.js' <<< "$page")
curl -fsS --max-time 30 "$web$asset" > "${RUNNER_TEMP:-/tmp}/tareas-bundle.js"
grep -Fq "$api" "${RUNNER_TEMP:-/tmp}/tareas-bundle.js"

headers=$(curl -fsS --max-time 15 -D - -o /dev/null -X OPTIONS "$path" \
  -H "Origin: $web" -H 'Access-Control-Request-Method: PATCH' \
  -H 'Access-Control-Request-Headers: x-task-board,content-type')
grep -Eiq '^access-control-allow-origin: \*|^access-control-allow-origin: https?://' <<< "$headers"
grep -Eiq '^access-control-allow-methods: .*PATCH' <<< "$headers"

id=$(curl -fsS --max-time 30 -H "$header" -H 'Content-Type: application/json' \
  -d '{"title":"Verificación automática"}' "$path" | jq -er '.id')
curl -fsS --max-time 30 -X PATCH -H "$header" -H 'Content-Type: application/json' \
  -d '{"title":"Verificación actualizada","completed":true}' "$path/$id" |
  jq -e '.completed and .title == "Verificación actualizada"' >/dev/null
curl -fsS --max-time 30 -H "$header" "$path?completed=true" |
  jq -e --arg id "$id" '.completed == 1 and .items[0].id == $id' >/dev/null
curl -fsS --max-time 30 -H "X-Task-Board: $(cat /proc/sys/kernel/random/uuid)" "$path" |
  jq -e '.total == 0' >/dev/null
curl -fsS --max-time 30 -X PATCH -H "$header" -H 'Content-Type: application/json' \
  -d '{"completed":false}' "$path/$id" | jq -e '.completed == false' >/dev/null
curl -fsS --max-time 30 -X DELETE -H "$header" "$path/$id" >/dev/null
id=''
curl -fsS --max-time 30 -H "$header" "$path" | jq -e '.total == 0' >/dev/null
echo 'Verificación de web, API, persistencia y aislamiento completada.'
