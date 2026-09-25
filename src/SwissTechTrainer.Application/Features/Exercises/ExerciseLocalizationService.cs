using System.Globalization;
using SwissTechTrainer.Domain.Enums;

namespace SwissTechTrainer.Application.Features.Exercises;

/// <summary>
/// Provides multi-language localization for technical interview challenges, markdown problem specifications, and diagnostic hints.
/// </summary>
public interface IExerciseLocalizationService
{
    /// <summary>
    /// Localizes the exercise title based on the specified culture or the ambient UI culture.
    /// </summary>
    string LocalizeTitle(CategoryType category, DifficultyLevel level, string originalTitle, string? cultureCode = null);

    /// <summary>
    /// Localizes the exercise problem description markdown based on the specified culture or the ambient UI culture.
    /// </summary>
    string LocalizeDescription(CategoryType category, DifficultyLevel level, string originalDescription, string? cultureCode = null);

    /// <summary>
    /// Localizes the exercise hints markdown based on the specified culture or the ambient UI culture.
    /// </summary>
    string LocalizeHints(CategoryType category, DifficultyLevel level, string originalHints, string? cultureCode = null);
}

/// <summary>
/// Default implementation of <see cref="IExerciseLocalizationService"/> supporting English (en), Spanish (es), and German (de).
/// </summary>
public class ExerciseLocalizationService : IExerciseLocalizationService
{
    public string LocalizeTitle(CategoryType category, DifficultyLevel level, string originalTitle, string? cultureCode = null)
    {
        var culture = NormalizeCulture(cultureCode);
        if (culture == "en") return originalTitle;

        if (category == CategoryType.Coding && level == DifficultyLevel.Level1)
        {
            return culture switch
            {
                "es" => "Agregador de Ventana Deslizante para Flujo de Órdenes Pragmático",
                "de" => "Pragmatischer Gleitfenster-Aggregator für Auftragsströme",
                _ => originalTitle
            };
        }

        if (category == CategoryType.SystemDesign && level == DifficultyLevel.Level1)
        {
            return culture switch
            {
                "es" => "Servicio Suizo de Notificación y Conciliación de Pagos QR-Bill",
                "de" => "Schweizer QR-Rechnung Zahlungsbenachrichtigungs- & Abstimmungsdienst",
                _ => originalTitle
            };
        }

        if (category == CategoryType.LanguageDeepDive && level == DifficultyLevel.Level1)
        {
            return culture switch
            {
                "es" => "Parser de Mensajes Financieros de Alto Rendimiento y Cero Asignación",
                "de" => "Hochdurchsatz- und Null-Allokations-Parser für Finanznachrichten",
                _ => originalTitle
            };
        }

        if (category == CategoryType.CleanCode && level == DifficultyLevel.Level1)
        {
            return culture switch
            {
                "es" => "Refactorización de Calculadora de Tarifas God-Class en Gestión Patrimonial",
                "de" => "Refaktorisierung der God-Class Gebührenrechner im Wealth Management",
                _ => originalTitle
            };
        }

        if (category == CategoryType.ApiDesign && level == DifficultyLevel.Level1)
        {
            return culture switch
            {
                "es" => "Endpoint de API RESTful Idempotente para Transferencias Patrimoniales",
                "de" => "Idempotenter RESTful-API-Endpunkt für Vermögensüberweisungen",
                _ => originalTitle
            };
        }

        if (category == CategoryType.Testing && level == DifficultyLevel.Level1)
        {
            return culture switch
            {
                "es" => "Implementación TDD del Validador de IBAN Suizo y Luhn Módulo 97",
                "de" => "TDD-Implementierung des Schweizer IBAN-Prüfers & Luhn Modulo 97",
                _ => originalTitle
            };
        }

        if (category == CategoryType.Behavioral && level == DifficultyLevel.Level1)
        {
            return culture switch
            {
                "es" => "Resolución de Conflictos de Arquitectura y Resistencia a la Migración Legacy",
                "de" => "Lösung von Architekturkonflikten & Widerstand gegen Legacy-Migration",
                _ => originalTitle
            };
        }

        return originalTitle;
    }

    public string LocalizeDescription(CategoryType category, DifficultyLevel level, string originalDescription, string? cultureCode = null)
    {
        var culture = NormalizeCulture(cultureCode);
        if (culture == "en") return originalDescription;

        if (category == CategoryType.Coding && level == DifficultyLevel.Level1)
        {
            if (culture == "es")
            {
                return """
### Bolsa Financiera de Zúrich — Rastreador de Tasa y Volumen del Flujo de Órdenes

En las plataformas suizas de negociación electrónica y fintech, el seguimiento de volúmenes de órdenes con ventanas deslizantes y una mínima asignación de memoria es esencial.

#### Especificación del Problema
Implemente un `OrderVolumeTracker` en memoria que registre órdenes de negociación con marca de tiempo en milisegundos y calcule:
1. El volumen total de operaciones ejecutadas en los últimos `W` milisegundos (ventana deslizante).
2. El número de operaciones dentro de la ventana.

#### Restricciones
- La complejidad temporal para `RecordOrder` y `GetWindowVolume` debe ser $O(1)$ amortizado.
- Se requiere seguridad en hilos (thread-safety para lecturas/escrituras concurrentes).
- Las asignaciones de memoria en la ruta crítica (hot path) deben ser mínimas.
""";
            }
            if (culture == "de")
            {
                return """
### Zürcher Finanzbörse — Auftragsfluss-Raten- & Volumen-Tracker

In Schweizer elektronischen Handels- und Fintech-Plattformen ist die Verfolgung von Gleitfenster-Auftragsvolumina mit minimaler Speicherallokation unerlässlich.

#### Problemspezifikation
Implementieren Sie einen In-Memory-`OrderVolumeTracker`, der Handelsaufträge mit Millisekunden-Zeitstempel aufzeichnet und berechnet:
1. Das Gesamtvolumen der in den letzten `W` Millisekunden ausgeführten Trades (Gleitfenster).
2. Die Anzahl der Trades innerhalb des Fensters.

#### Einschränkungen
- Die Laufzeitkomplexität für `RecordOrder` und `GetWindowVolume` muss amortisiert $O(1)$ sein.
- Thread-Sicherheit ist erforderlich (paralleler Lese-/Schreibzugriff).
- Speicherallokationen im Hot Path müssen minimal sein.
""";
            }
        }

        if (category == CategoryType.SystemDesign && level == DifficultyLevel.Level1)
        {
            if (culture == "es")
            {
                return """
### Servicio Suizo de Notificación de Pagos Interbancarios (ISO 20022 / QR-Bill)

Está diseñando un microservicio backend de conciliación para un banco cantonal privado suizo que procesa notificaciones de pago QR-bill recibidas a través de SIX Interbank Clearing (SIC).

#### Requisitos
1. Ingerir notificaciones de pago desde la pasarela de compensación (hasta 2,000 notificaciones/seg durante el pico matutino de liquidación).
2. Garantizar procesamiento exactamente una vez (Idempotencia) contra webhooks duplicados de compensación.
3. Desacoplar la ingesta de notificaciones de las actualizaciones en el core bancario del libro mayor.
4. Proporcionar justificación arquitectónica estructurada y diagrama de componentes ASCII/Mermaid.

#### Entregable
Proporcione un documento conciso de diseño arquitectónico con:
- Diagrama de componentes (Ingress API, Message Broker, Almacén de Idempotencia, Outbox/Worker, Core Bancario).
- Elección de almacenamiento y estrategia de particionamiento.
- Modos de fallo y recuperación (Dead Letter Queue, manejo de mensajes venenosos).
""";
            }
            if (culture == "de")
            {
                return """
### Schweizer Interbank-Zahlungsbenachrichtigungsdienst (ISO 20022 / QR-Rechnung)

Sie entwerfen einen Backend-Abstimmungs-Mikrodienst für eine Schweizer Privat-/Kantonalbank, der QR-Rechnungszahlungen verarbeitet, die über SIX Interbank Clearing (SIC) eingehen.

#### Anforderungen
1. Erfassen von Zahlungsbenachrichtigungen vom Clearing-Gateway (bis zu 2.000 Benachrichtigungen/Sek. während der morgendlichen Abrechnungsspitze).
2. Gewährleistung der Exactly-Once-Verarbeitung (Idempotenz) gegen doppelte Clearing-Webhooks.
3. Entkopplung der Erfassung von Benachrichtigungen von nachgelagerten Kernbank-Buchungsaktualisierungen.
4. Strukturierte Architekturbegründung und ASCII/Mermaid-Komponentendiagramm bereitstellen.

#### Ergebnis
Bereitstellung eines prägnanten Architekturdesign-Dokuments mit:
- Komponentendiagramm (Ingress-API, Message Broker, Idempotenzspeicher, Outbox/Worker, Kernbank).
- Speicherwahl & Partitionierungsstrategie.
- Fehlermodi & Wiederherstellung (Dead-Letter-Queue, Poison-Message-Behandlung).
""";
            }
        }

        if (category == CategoryType.LanguageDeepDive && level == DifficultyLevel.Level1)
        {
            if (culture == "es")
            {
                return """
### Optimización de Memoria CLR y Span<T>

Un parser de feeds de mercado heredado asigna cientos de strings pequeños por mensaje FIX/JSON, causando pausas de GC altas en Gen 0/Gen 1 durante la apertura del mercado.

#### Requisitos
1. Refactorice el parser para utilizar `ReadOnlySpan<char>` y segmentación de `Span<T>` en lugar de `string.Substring()` y `string.Split()`.
2. Analice pares clave-valor (p. ej., `Tag=Value;Tag2=Value2`) sin asignar strings en el montículo (heap) para claves ni valores numéricos.
3. Garantice cero asignaciones de memoria en el bucle de análisis.
""";
            }
            if (culture == "de")
            {
                return """
### CLR-Speicher- & Span<T>-Optimierung

Ein bestehender Marktfeed-Parser allokiert Hunderte kleiner Zeichenketten pro FIX/JSON-Nachricht, was bei Markteröffnung hohe GC-Pausen in Gen 0/Gen 1 verursacht.

#### Anforderungen
1. Refaktorisieren Sie den Parser so, dass er `ReadOnlySpan<char>` und `Span<T>`-Slicing anstelle von `string.Substring()` und `string.Split()` verwendet.
2. Parsen von Schlüssel-Wert-Paaren (z. B. `Tag=Value;Tag2=Value2`), ohne Heap-Strings für Schlüssel oder numerische Werte zu allokieren.
3. Stellen Sie sicher, dass in der Parsing-Schleife keine Speicherallokationen stattfinden.
""";
            }
        }

        if (category == CategoryType.CleanCode && level == DifficultyLevel.Level1)
        {
            if (culture == "es")
            {
                return """
### Refactorización del Motor de Tarifas de Gestión Patrimonial

La siguiente clase viola el Principio de Responsabilidad Única y Abierto/Cerrado, tiene una alta complejidad ciclomática, llamadas directas a base de datos/correo y casos límite no gestionados.

#### Tareas
1. Refactorice en servicios de dominio limpios y desacoplados utilizando Estrategia e Inyección de Dependencias.
2. Separe las reglas de cálculo (p. ej., Tarifas de Custodia Escalonadas, Tarifas de Transacción, Deducciones Fiscales) para cumplir con OCP.
3. Agregue validación defensiva robusta.
""";
            }
            if (culture == "de")
            {
                return """
### Refaktorisierung der Wealth-Management-Gebühren-Engine

Die folgende Klasse verletzt Single Responsibility und das Open/Closed-Prinzip, weist eine hohe zyklomatische Komplexität auf, enthält fest codierte Datenbank-/E-Mail-Aufrufe und unbehandelte Randfälle.

#### Aufgaben
1. Refaktorisieren Sie in saubere, entkoppelte Domänendienste unter Verwendung von Strategie und Dependency Injection.
2. Trennen Sie die Berechnungsregeln (z. B. gestaffelte Depotgebühren, Transaktionsgebühren, Steuerabzüge), um OCP einzuhalten.
3. Fügen Sie eine robuste defensive Validierung hinzu.
""";
            }
        }

        if (category == CategoryType.ApiDesign && level == DifficultyLevel.Level1)
        {
            if (culture == "es")
            {
                return """
### Controlador de API de Transferencias SEPA / SIC Suizas Idempotente

Diseñe e implemente un endpoint robusto de ASP.NET Core Minimal API o Controlador para transferencias de fondos que garantice la idempotencia a través del encabezado HTTP `Idempotency-Key`.

#### Requisitos
1. Aceptar `POST /api/v1/transfers` con cuerpo `{ sourceIban, targetIban, amount, currency }`.
2. Extraer el encabezado `Idempotency-Key`; si falta, devolver `400 Bad Request`.
3. Si una clave de idempotencia se recibe por primera vez, procesar la transferencia y almacenar en caché la respuesta.
4. Si la misma clave se vuelve a enviar con el mismo payload, devolver el resultado en caché con estado `200 OK` y encabezado personalizado `X-Cache-Lookup: Hit`.
5. Si la misma clave se vuelve a enviar con un payload diferente en conflicto, devolver `409 Conflict`.
""";
            }
            if (culture == "de")
            {
                return """
### Idempotenter Schweizer SEPA / SIC Überweisungs-API-Controller

Entwerfen und implementieren Sie einen robusten ASP.NET Core Minimal-API-Endpunkt oder Controller für Überweisungen, der Idempotenz über den HTTP-Header `Idempotency-Key` garantiert.

#### Anforderungen
1. Akzeptieren von `POST /api/v1/transfers` mit Body `{ sourceIban, targetIban, amount, currency }`.
2. Extrahieren des `Idempotency-Key`-Headers; falls fehlend, `400 Bad Request` zurückgeben.
3. Wenn ein Idempotenz-Schlüssel zum ersten Mal empfangen wird, Überweisung verarbeiten und Antwort zwischenspeichern.
4. Wenn derselbe Schlüssel mit exakt derselben Nutzlast erneut gesendet wird, zwischengespeichertes Ergebnis mit `200 OK` und Header `X-Cache-Lookup: Hit` zurückgeben.
5. Wenn derselbe Schlüssel mit einer widersprüchlichen Nutzlast erneut gesendet wird, `409 Conflict` zurückgeben.
""";
            }
        }

        if (category == CategoryType.Testing && level == DifficultyLevel.Level1)
        {
            if (culture == "es")
            {
                return """
### Validación de Checksum de IBAN Suizo (CH / LI) mediante TDD

Escriba pruebas unitarias integrales y la implementación de producción para validar números IBAN de Suiza (`CH...`, 21 caracteres) y Liechtenstein (`LI...`, 21 caracteres) de acuerdo con ISO 13616 y MOD 97-10 (ISO 7064).

#### Requisitos
1. Cubrir IBANs suizos válidos, IBANs de Liechtenstein, longitudes inválidas, prefijos de país incorrectos y dígitos de control erróneos.
2. Seguir una estructura estricta de pruebas AAA con nombres descriptivos.
""";
            }
            if (culture == "de")
            {
                return """
### Schweizer IBAN (CH / LI) Prüfziffernvalidierung via TDD

Schreiben Sie umfassende Unit-Tests und die Produktionsimplementierung zur Validierung von Schweizer (`CH...`, 21 Zeichen) und Liechtenstein (`LI...`, 21 Zeichen) IBAN-Nummern gemäß ISO 13616 und MOD 97-10 (ISO 7064).

#### Anforderungen
1. Abdeckung gültiger Schweizer IBANs, Liechtenstein-IBANs, ungültiger Längen, falscher Ländervorwahlen und fehlerhafter Prüfziffern.
2. Befolgen einer strengen AAA-Teststruktur mit aussagekräftigen Testnamen.
""";
            }
        }

        if (category == CategoryType.Behavioral && level == DifficultyLevel.Level1)
        {
            if (culture == "es")
            {
                return """
### Escenario de Tech Lead Empresarial Suizo: Modernización vs Plazos de Entrega

#### Escenario
Se ha incorporado a una firma privada de gestión patrimonial en Zúrich como Tech Lead. El equipo está dividido: los ingenieros senior desean pausar el desarrollo de funcionalidades durante 4 meses para reescribir un monolito legacy a microservicios basados en eventos. La dirección de producto y los interesados del negocio insisten en entregar funciones de cumplimiento normativo MIFID II en un plazo muy estricto.

#### Tarea
Formule su respuesta utilizando el **método STAR (Situación, Tarea, Acción, Resultado)** demostrando:
1. Liderazgo técnico pragmático y mitigación de riesgos (p. ej., patrón Strangler Fig en lugar de reescritura total).
2. Negociación con partes interesadas y comunicación transparente de trade-offs.
3. Mentoría del equipo y establecimiento de prácticas de arquitectura evolutiva.
""";
            }
            if (culture == "de")
            {
                return """
### Schweizer Enterprise Tech-Lead-Szenario: Modernisierung vs. Lieferfristen

#### Szenario
Sie sind als Tech Lead zu einer Zürcher Vermögensverwaltungsgesellschaft gestoßen. Das Team ist gespalten: Senior Engineers möchten die Feature-Bereitstellung für 4 Monate stoppen, um einen bestehenden Monolithen in ereignisgesteuerte Microservices umzuschreiben. Das Produktmanagement und die Business-Stakeholder bestehen auf der Einhaltung regulatorischer MIFID-II-Compliance-Fristen.

#### Aufgabe
Formulieren Sie Ihre Antwort nach der **STAR-Methode (Situation, Task, Action, Result)**:
1. Pragmatische technische Führung und Risikominderung (z. B. Strangler-Fig-Muster statt Big-Bang-Neuschreiben).
2. Verhandlung mit Stakeholdern und transparente Kommunikation von Kompromissen.
3. Mentoring des Teams und Etablierung evolutionärer Architekturpraktiken.
""";
            }
        }

        // Generic Markdown header translation fallback for dynamic or unlisted exercises
        return LocalizeGenericMarkdown(originalDescription, culture);
    }

    public string LocalizeHints(CategoryType category, DifficultyLevel level, string originalHints, string? cultureCode = null)
    {
        var culture = NormalizeCulture(cultureCode);
        if (culture == "en") return originalHints;

        if (category == CategoryType.Coding && level == DifficultyLevel.Level1)
        {
            return culture switch
            {
                "es" => "Considere utilizar un búfer circular seguro para hilos o ConcurrentQueue con marcas de tiempo monotónicas. Descarte entradas expiradas durante consultas o registros.",
                "de" => "Erwägen Sie die Verwendung eines threadsicheren Ringpuffers oder einer ConcurrentQueue mit monotonen Zeitstempeln. Entfernen Sie abgelaufene Einträge bei Abfragen oder Aufzeichnungen.",
                _ => originalHints
            };
        }

        if (category == CategoryType.SystemDesign && level == DifficultyLevel.Level1)
        {
            return culture switch
            {
                "es" => "Enfóquese en el patrón Outbox combinado con tablas de idempotencia transaccional. Mencione los requisitos de auditoría y cumplimiento regulatorio bancario suizo.",
                "de" => "Konzentrieren Sie sich auf das Outbox-Muster in Kombination mit transaktionalen Idempotenztabellen. Erwähnen Sie die regulatorischen Audit-Logging-Anforderungen des Schweizer Bankwesens.",
                _ => originalHints
            };
        }

        if (category == CategoryType.LanguageDeepDive && level == DifficultyLevel.Level1)
        {
            return culture switch
            {
                "es" => "Utilice MemoryExtensions.IndexOf y Span<char>.Slice. Utilice decimal.TryParse e int.TryParse directamente sobre ReadOnlySpan<char>.",
                "de" => "Verwenden Sie MemoryExtensions.IndexOf und Span<char>.Slice. Verwenden Sie decimal.TryParse und int.TryParse direkt auf ReadOnlySpan<char>.",
                _ => originalHints
            };
        }

        if (category == CategoryType.CleanCode && level == DifficultyLevel.Level1)
        {
            return culture switch
            {
                "es" => "Extraiga una interfaz IFeeStrategy por tipo de cliente. Extraiga INotificationService. Haga los cálculos puros y testeables.",
                "de" => "Extrahieren Sie ein IFeeStrategy-Interface pro Kundentyp. Extrahieren Sie INotificationService. Machen Sie Berechnungen rein und testbar.",
                _ => originalHints
            };
        }

        if (category == CategoryType.ApiDesign && level == DifficultyLevel.Level1)
        {
            return culture switch
            {
                "es" => "Utilice un IIdempotencyStore (en memoria o caché distribuida) que almacene el hash del payload de la solicitud y la respuesta.",
                "de" => "Verwenden Sie einen IIdempotencyStore (In-Memory oder verteilter Cache), der den Hash der Anforderungsnutzlast und die Antwort speichert.",
                _ => originalHints
            };
        }

        if (category == CategoryType.Testing && level == DifficultyLevel.Level1)
        {
            return culture switch
            {
                "es" => "Mueva los primeros 4 caracteres al final, convierta letras en números (A=10, Z=35) y calcule Módulo 97 (el residuo debe ser igual a 1).",
                "de" => "Verschieben Sie die ersten 4 Zeichen an das Ende, konvertieren Sie Buchstaben in Zahlen (A=10, Z=35) und berechnen Sie Modulo 97 (der Rest muss 1 sein).",
                _ => originalHints
            };
        }

        if (category == CategoryType.Behavioral && level == DifficultyLevel.Level1)
        {
            return culture switch
            {
                "es" => "Evite posturas extremas. Enfóquese en arquitectura evolutiva (Strangler Fig), alineación con el valor de negocio y seguridad psicológica para el equipo de ingeniería.",
                "de" => "Vermeiden Sie extreme Haltungen. Konzentrieren Sie sich auf evolutionäre Architektur (Strangler Fig), Ausrichtung auf Geschäftswerte und psychologische Sicherheit für das Engineering-Team.",
                _ => originalHints
            };
        }

        return originalHints;
    }

    private static string NormalizeCulture(string? cultureCode)
    {
        if (string.IsNullOrWhiteSpace(cultureCode))
        {
            cultureCode = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        }

        var lower = cultureCode.Trim().ToLowerInvariant();
        if (lower.StartsWith("es")) return "es";
        if (lower.StartsWith("de")) return "de";
        return "en";
    }

    private static string LocalizeGenericMarkdown(string markdown, string culture)
    {
        if (string.IsNullOrWhiteSpace(markdown)) return markdown;

        if (culture == "es")
        {
            return markdown
                .Replace("#### Problem Specification", "#### Especificación del Problema")
                .Replace("#### Constraints", "#### Restricciones")
                .Replace("#### Requirements", "#### Requisitos")
                .Replace("#### Tasks", "#### Tareas")
                .Replace("#### Scenario", "#### Escenario")
                .Replace("#### Deliverable", "#### Entregable")
                .Replace("#### Deliverables", "#### Entregables")
                .Replace("#### Background", "#### Contexto")
                .Replace("#### Implementation Notes", "#### Notas de Implementación");
        }

        if (culture == "de")
        {
            return markdown
                .Replace("#### Problem Specification", "#### Problemspezifikation")
                .Replace("#### Constraints", "#### Einschränkungen")
                .Replace("#### Requirements", "#### Anforderungen")
                .Replace("#### Tasks", "#### Aufgaben")
                .Replace("#### Scenario", "#### Szenario")
                .Replace("#### Deliverable", "#### Ergebnis")
                .Replace("#### Deliverables", "#### Ergebnisse")
                .Replace("#### Background", "#### Hintergrund")
                .Replace("#### Implementation Notes", "#### Implementierungshinweise");
        }

        return markdown;
    }
}
