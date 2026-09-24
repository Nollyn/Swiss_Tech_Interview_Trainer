# ADR 0004: Arquitectura Multi-idioma de Interfaz (I18N) y Soporte Multi-lenguaje de Programación en el Pipeline de IA

## Status
Accepted

## Context
La plataforma *Swiss Tech Interview Trainer* fue concebida inicialmente acoplada de forma exclusiva al ecosistema .NET/C# y restringida idiomáticamente al idioma inglés. Sin embargo, para posicionar la solución de forma competitiva en el mercado internacional —con especial foco en el mercado tecnológico suizo caracterizado por su diversidad lingüística y tecnológica—, se identificaron dos requisitos arquitectónicos críticos:

1. **Internacionalización de la Interfaz (I18N UI)**: Requerimiento de soporte multi-idioma nativo para Inglés (en-US), Alemán (de-CH / de-DE) y Español (es-ES), garantizando cambios de idioma fluidos e inmediatos sin fricción en la experiencia de usuario.
2. **Evaluación Políglota de Backend en el Pipeline de IA**: Capacidad de evaluar candidatos en múltiples stacks tecnológicos de backend (`C#`, `Python`, `Java`, `Rust`, `Go`, `Node.js`), adaptando dinámicamente el comportamiento del modelo de lenguaje (LLM) y las rúbricas de evaluación técnica, preservando la pureza y los límites del diseño de Clean Architecture.

## Decision
Se implementa una estrategia de desacoplamiento en dos niveles claramente diferenciados:

### 1. Nivel UI (Internacionalización - I18N)
- **Middleware de Localización ASP.NET Core**: Se adopta el mecanismo estándar de localización de ASP.NET Core utilizando archivos de recursos fuertemente tipados (`.resx`) organizados jerárquicamente en la capa Web/UI (`Resources/`).
- **Persistencia de Preferencia Cultural**: La cultura seleccionada por el usuario se gestiona y persiste mediante cookies de cultura estándar (`CookieRequestCultureProvider`), permitiendo la conmutación en caliente (*hot-swapping*) en Blazor Server y manteniendo la consistencia cultural a través de reconexiones de Circuitos SignalR.

### 2. Nivel Dominio e Pipeline de IA (Soporte Multi-lenguaje)
- **Modelado en el Dominio**: Se introduce `ProgrammingLanguage` como un concepto de primer nivel (Value Object / Enumeración enriquecida) en la capa de Dominio, desvinculando la lógica de ejercicios y evaluaciones de una tecnología fija.
- **Pipeline Evaluator-Optimizer Contextualizado**: En la capa de Aplicación, el pipeline del Evaluador-Optimizer intercepta el `ProgrammingLanguage` seleccionado para inyectar dinámicamente plantillas de prompts de ingeniería especializadas hacia el cliente LLM (`Groq` / `Ollama`).
- **Rúbricas Dinámicas por Ecosistema**: Las rúbricas de evaluación técnica se adaptan según el lenguaje analizado (por ejemplo, validación de gestión de memoria y *Borrow Checker* en Rust, recolección de basura y concurrencia sobre la JVM en Java, o tipado estático y *async/await* en C#/TypeScript en lugar de asumir invariantes de .NET).
- **Aislamiento de Métricas y Progresión**: El progreso del candidato, historial de intentos y puntuaciones deterministas se segregan y aíslan independientemente por cada lenguaje de programación.

## Consequences

### Positivas
- **Extensibilidad Abierta/Cerrada (OCP)**: Flexibilidad total para incorporar nuevos lenguajes de backend en el futuro extendiendo las definiciones de plantillas y rúbricas en Infrastructure/LLM sin alterar el core del dominio.
- **Estándares Empresariales de Localización**: Adopción de patrones de localización nativos y mantenibles de .NET 10, facilitando traducciones adicionales sin refactorizar vistas.
- **Aislamiento Estricto de Métricas**: Métricas de rendimiento, maestría técnica y analítica de candidatos segmentadas con precisión por tecnología.

### Negativas / Trade-offs & Mitigaciones
- **Sobrecarga de Mantenimiento en Prompts**: Mayor complejidad en el ciclo de vida y versionado de las plantillas de prompts de LLM especializadas para cada lenguaje.
  - *Mitigación*: Centralización de templates de prompts en `LlmPromptTemplates` con pruebas automatizadas de generación y validación de esquemas de salida JSON.
- **Complejidad en Pruebas de Integración**: Incremento en la matriz de pruebas del pipeline evaluador al requerir validaciones contra múltiples sintaxis y rúbricas tecnológicas.
  - *Mitigación*: Uso del `DeterministicMockLlmClient` con suites de pruebas parametrizadas por `ProgrammingLanguage` para asegurar cobertura exhaustiva sin incurrir en latencia ni consumo de APIs externas.
