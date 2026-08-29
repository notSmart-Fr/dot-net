TaskApi/
├── Core/
│   ├── Exceptions/
│   │   ├── DomainException.cs           <── Step 1: Base Business Exception
│   │   └── TaskNotFoundException.cs     <── Typed Business Rule Exceptions
│   └── Interfaces/
│       ├── IExceptionMapper.cs          <── Step 2: Open-Closed Strategy Contract
│       └── IEndpoint.cs                 <── Step 3: Route Discovery Marker
│
├── Infrastructure/
│   ├── ExceptionHandling/
│   │   ├── GlobalExceptionHandler.cs    <── Step 4: Closed Execution Engine
│   │   └── ExceptionExtensions.cs       <── Step 5: Scrutor Discovery Setup
│   ├── Database/
│   │   ├── DatabaseExtensions.cs        <── Step 6: EF Core & Postgres Setup
│   │   └── DbExceptionMapper.cs         <── Step 7: Co-located DB Error Mapping
│   ├── Auth/
│   │   ├── AuthExtensions.cs            <── Step 8: Supabase JWT Setup
│   │   └── AuthExceptionMapper.cs       <── Step 9: Co-located Auth Error Mapping
│   ├── Caching/
│   │   └── RedisExtensions.cs           <── Step 10: Redis & Data Protection Setup
│   ├── ApiDocs/
│   │   └── ApiDocsExtensions.cs         <── Step 11: OpenAPI & Scalar UI
│   └── Extensions/
│       ├── EndpointExtensions.cs        <── Step 12: Auto-route Discovery Engine
│       └── FeatureExtensions.cs         <── Step 13: Auto-handler Registration
│
├── Common/
│   ├── Filters/
│   │   └── ValidationFilter.cs          <── Step 14: FluentValidation Pipeline Filter
│   └── Mappers/
│       └── BadRequestExceptionMapper.cs <── Step 15: Bad Request / JSON Syntax Mapper
│
└── Program.cs                           <── Step 16: Immutable Orchestrator (< 30 lines)