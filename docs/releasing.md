# Releasing

## Paket

| Paket | Beschreibung | Workflow |
|---|---|---|
| `Qml4Net` | QGIS QML Layer-Style Codec | `publish-qml4net.yml` |

Das Paket wird auf [nuget.org](https://www.nuget.org/) veroeffentlicht.

## Voraussetzungen

- `NUGET_API_KEY` muss als Repository-Secret in den GitHub-Settings hinterlegt sein
  (Settings > Secrets and variables > Actions > Repository secrets)
- Das Secret muss in der GitHub Environment `nuget` verfuegbar sein
  (Settings > Environments > nuget)
- Der Key muss Berechtigungen fuer `Qml4Net` auf nuget.org haben

## Release per Git-Tag

Der Publish-Workflow wird automatisch ausgeloest wenn ein Tag mit dem
Praefix `Qml4Net-v` gepusht wird.

```bash
# Sicherstellen dass main aktuell ist
git checkout main
git pull

# Tag erstellen und pushen
git tag Qml4Net-v0.1.0
git push origin Qml4Net-v0.1.0
```

### Tag-Format

| Paket | Tag-Muster | Beispiele |
|---|---|---|
| `Qml4Net` | `Qml4Net-v<semver>` | `Qml4Net-v0.1.0`, `Qml4Net-v1.0.0`, `Qml4Net-v2.0.0-rc.1` |

Die Version muss gueltigem SemVer entsprechen (`major.minor.patch` mit
optionalem Pre-Release-Suffix).

## Release per workflow_dispatch

Alternativ kann ein Release manuell ueber die GitHub Actions UI ausgeloest
werden, ohne einen Tag zu erstellen:

1. GitHub > Actions > "Publish Qml4Net"
2. "Run workflow" klicken
3. Version eingeben (z.B. `0.1.0`)
4. "Run workflow" bestaetigen

Dies ist nuetzlich fuer Testveroeffentlichungen oder wenn kein Tag
erwuenscht ist.

## Was der Workflow macht

Der Publish-Workflow fuehrt folgende Schritte aus:

1. **Checkout** -- Repository auschecken
2. **Version ermitteln** -- aus dem Tag-Namen oder dem `workflow_dispatch`-Input
3. **Test** -- `docker buildx build --target test` (vollstaendiger Testlauf mit Coverage-Gate)
4. **Pack** -- `docker buildx build --target artifacts` mit `PACK_TARGET` und `PACKAGE_VERSION`
5. **Upload** -- `.nupkg`-Datei als GitHub Actions Artifact sichern
6. **Push** -- `docker buildx build --target push` mit `NUGET_API_KEY` als BuildKit-Secret

Der Push verwendet `--skip-duplicate`, sodass ein erneutes Ausloesen mit
derselben Version keinen Fehler erzeugt.

## Versionierung

Das Projekt folgt [Semantic Versioning](https://semver.org/):

- **0.x.y** -- Initiale Entwicklung, API kann sich aendern
- **1.0.0** -- Erste stabile API
- **Major** -- Breaking Changes
- **Minor** -- Neue Features, abwaertskompatibel
- **Patch** -- Bugfixes, abwaertskompatibel

## Lokales Testen des Pack-Schritts

**Wichtig:** Immer `--target artifacts` verwenden (nicht `--target pack`).
Der `artifacts`-Stage basiert auf `FROM scratch` und enthaelt nur die
`.nupkg`-Dateien. Der `pack`-Stage wuerde das gesamte SDK-Dateisystem
(~1 GB) exportieren.

```bash
docker buildx build --target artifacts \
  --build-arg PACK_TARGET=src/Qml4Net/Qml4Net.csproj \
  --build-arg PACKAGE_VERSION=0.1.0 \
  -o type=local,dest=./artifacts .
```

Die `.nupkg`-Datei landet im `./artifacts/`-Verzeichnis (wenige KB).

## Checkliste vor einem Release

- [ ] Alle Tests gruen (`docker buildx build --target test .`)
- [ ] CHANGELOG.md aktualisiert (Unreleased > Versionsnummer + Datum)
- [ ] README.md Statusabschnitt aktuell
- [ ] Keine uncommitteten Aenderungen (`git status`)
- [ ] Main-Branch ist aktuell (`git pull`)
- [ ] Version noch nicht auf nuget.org vorhanden

## Wichtig: CHANGELOG vor dem Tag aktualisieren

Der Publish-Workflow baut das Paket aus dem Commit auf den der Tag zeigt.
Die CHANGELOG.md muss daher **vor** dem Erstellen des Tags committet und
gepusht werden, damit sie im Paket enthalten ist.

Reihenfolge:

1. CHANGELOG.md mit neuem Versionsabschnitt committen
2. `git push origin main`
3. Tag erstellen und pushen

Wenn die CHANGELOG erst nach dem Tag aktualisiert wird, enthaelt das
veroeffentlichte Paket die alte CHANGELOG.

## Bisherige Releases

Noch keine Releases.
