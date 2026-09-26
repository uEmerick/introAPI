# 📋 Resumo dos Endpoints da API - Cidades e Alunos

## ✅ Status do Projeto
- **Build**: ✅ Sucesso (0 erros, 31 warnings pré-existentes)
- **Serilog**: ✅ Configurado (logs em arquivo)
- **OpenAPI**: ✅ Disponível em `/doc`

---

## 🏙️ ENDPOINTS DE CIDADES

### 1. **POST /api/cidades/importar**
Importa cidades a partir de um arquivo CSV

**Request:**
```
Content-Type: multipart/form-data
Body: arquivo CSV (CidadeId, Nome, Sigla, IBGEMunicipio, Latitude, Longitude)
```

**Response (200):**
```json
{
  "mensagem": "Cidades importadas com sucesso!"
}
```

**Validações:**
- ✅ Arquivo não pode ser nulo
- ✅ Arquivo deve ser .csv
- ✅ Tamanho máximo: 10MB
- ✅ CSV deve ter header
- ✅ CidadeId, IBGEMunicipio devem ser > 0
- ✅ Nome e Sigla não podem estar vazios

---

### 2. **GET /api/cidades**
Retorna todas as cidades

**Response (200):**
```json
[
  {
	"cidadeId": 1,
	"nome": "São Paulo",
	"sigla": "SP",
	"ibgeMunicipio": 3550308,
	"latitude": -23.5505,
	"longitude": -46.6333
  }
]
```

---

### 3. **GET /api/cidades/total**
Retorna a quantidade total de cidades

**Response (200):**
```json
{
  "total": 5570
}
```

---

### 4. **GET /api/cidades/{id}**
Retorna uma cidade pelo ID

**Response (200):**
```json
{
  "cidadeId": 1,
  "nome": "São Paulo",
  "sigla": "SP",
  "ibgeMunicipio": 3550308,
  "latitude": -23.5505,
  "longitude": -46.6333
}
```

**Response (404):**
```json
{
  "erro": "Cidade não encontrada"
}
```

---

### 5. **GET /api/cidades/estados**
Retorna todos os estados (UFs) únicos

**Response (200):**
```json
{
  "estados": ["AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"]
}
```

---

### 6. **GET /api/cidades/estado/{uf}**
Retorna todas as cidades de um estado específico

**Example:**
```
GET /api/cidades/estado/SP
```

**Response (200):**
```json
[
  {
	"cidadeId": 1,
	"nome": "São Paulo",
	"sigla": "SP",
	"ibgeMunicipio": 3550308,
	"latitude": -23.5505,
	"longitude": -46.6333
  }
]
```

---

### 7. **PUT /api/cidades/{id}**
Atualiza uma cidade

**Request Body:**
```json
{
  "cidadeId": 1,
  "nome": "São Paulo",
  "sigla": "SP",
  "ibgeMunicipio": 3550308,
  "latitude": -23.5505,
  "longitude": -46.6333
}
```

**Response (200):**
```json
{
  "mensagem": "Cidade atualizada com sucesso"
}
```

---

### 8. **DELETE /api/cidades/{id}**
Exclui uma cidade

**Response (200):**
```json
{
  "mensagem": "Cidade excluída com sucesso"
}
```

---

## 👨‍🎓 ENDPOINTS DE ALUNOS - FOTOS (Novo)

### 1. **POST /alunos/{id}/foto**
Faz upload da foto de um aluno

**Request:**
```
Content-Type: multipart/form-data
Body: arquivo de imagem (JPG, PNG, GIF, BMP)
```

**Response (200):**
```json
{
  "mensagem": "Foto enviada com sucesso"
}
```

**Validações:**
- ✅ Arquivo não pode ser nulo
- ✅ Apenas imagens permitidas (JPG, PNG, GIF, BMP)
- ✅ Tamanho máximo: 5MB

---

### 2. **GET /alunos/{id}/foto**
Retorna a foto de um aluno em base64

**Response (200):**
```json
{
  "foto": "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+P+/HgAFhAJ/wlseKgAAAABJRU5ErkJggg=="
}
```

**Response (404):**
```json
{
  "erro": "Foto não encontrada para este aluno"
}
```

---

### 3. **DELETE /alunos/{id}/foto**
Deleta a foto de um aluno

**Response (204):** No Content

---

## 📝 LOGGING (Serilog)

Todos os eventos são registrados em arquivo:
- **Localização**: `logs/aplicacao-YYYY-MM-DD.txt`
- **Formato**: `{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}`
- **Eventos**: Upload de fotos, importação de CSV, erros, warnings, etc.

---

## 📚 DOCUMENTAÇÃO (OpenAPI)

Acesse a documentação interativa em: `https://localhost:5001/doc`

---

## 🔍 EXEMPLOS COM cURL

### Importar CSV
```bash
curl -X POST "https://localhost:5001/api/cidades/importar" \
  -F "arquivo=@cidade.csv"
```

### Obter todas as cidades
```bash
curl -X GET "https://localhost:5001/api/cidades"
```

### Obter cidades por estado
```bash
curl -X GET "https://localhost:5001/api/cidades/estado/SP"
```

### Upload de foto
```bash
curl -X POST "https://localhost:5001/alunos/1/foto" \
  -F "arquivo=@foto.jpg"
```

### Obter foto em base64
```bash
curl -X GET "https://localhost:5001/alunos/1/foto"
```

---

## 📦 ESTRUTURA DO PROJETO

```
IntroAPI/
├── Controllers/
│   ├── CidadesController.cs       (8 endpoints)
│   ├── AlunosController.cs        (3 endpoints novos para foto)
│   └── ...
├── Services/
│   ├── CidadeService.cs           (Validações e lógica)
│   ├── AlunoService.cs            (Gerenciamento de fotos)
│   └── ...
├── Repository/
│   ├── CidadeRepository.cs        (Operações CRUD)
│   ├── AlunoRepository.cs         (Salvar/deletar fotos)
│   └── MySqlDbContext.cs
├── Entidades/
│   ├── Cidade.cs
│   └── Aluno.cs                   (Propriedade Foto adicionada)
├── Program.cs                      (Serilog configurado)
├── appsettings.json               (Serilog settings)
└── IntroAPI.csproj               (Dependências)
```

---

## 🎯 IMPLEMENTAÇÕES COMPLETADAS

### Parte 1: Endpoints de Cidades ✅
- [x] POST /api/cidades/importar
- [x] GET /api/cidades
- [x] GET /api/cidades/total
- [x] GET /api/cidades/{id}
- [x] GET /api/cidades/estados
- [x] GET /api/cidades/estado/{uf}
- [x] PUT /api/cidades/{id}
- [x] DELETE /api/cidades/{id}

### Parte 2: Fotos de Aluno ✅
- [x] POST /alunos/{id}/foto (upload)
- [x] GET /alunos/{id}/foto (retorna base64)
- [x] DELETE /alunos/{id}/foto (deletar)

### Parte 3: Documentação e Logging ✅
- [x] OpenAPI (documentação em /doc)
- [x] Serilog (logging em arquivo)

---

## ⚠️ NOTAS IMPORTANTES

1. **Banco de Dados**: Certifique-se de que a tabela `Aluno` possui a coluna `Foto` (LONGBLOB)
2. **CSV**: O arquivo deve usar `,` como delimiter (ou altere em CidadeService.cs)
3. **Logs**: Verifique a pasta `logs/` para monitorar eventos da aplicação
4. **Erros**: Todos os erros são registrados no Serilog para facilitar debugging

---

**Desenvolvido com ❤️ - Projeto Acadêmico**
