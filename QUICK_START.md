# 🚀 QUICK START - Próximos Passos

## 1️⃣ PREPARAR A API

```powershell
# No PowerShell, na pasta do projeto:
dotnet clean
dotnet build
dotnet run
```

**Esperado:**
```
info: Microsoft.Hosting.Lifetime[14]
	  Now listening on: https://localhost:5001
```

---

## 2️⃣ TESTAR IMPORTAÇÃO (Mais Fácil)

### Usando Postman/Thunder Client

1. **Criar requisição:**
   - Tipo: `POST`
   - URL: `https://localhost:5001/api/cidades/importar`

2. **Na aba Body:**
   - Selecione `form-data`
   - Key: `arquivo`
   - Value: Seu arquivo `cidade.csv`
   - Type: `file`

3. **Clicar Send**

4. **Resultado:**
   ```json
   {
	 "mensagem": "Cidades importadas com sucesso!"
   }
   ```

---

## 3️⃣ VALIDAR IMPORTAÇÃO

```bash
# Listar todas as cidades
GET https://localhost:5001/api/cidades

# Contar quantas cidades foram importadas
GET https://localhost:5001/api/cidades/total

# Listar cidades do RO
GET https://localhost:5001/api/cidades/estado/RO
```

---

## 4️⃣ DOCUMENTAÇÃO INTERATIVA

Abra no navegador:
```
https://localhost:5001/doc
```

Ali você pode testar todos os endpoints sem Postman!

---

## 5️⃣ MONITORAR LOGS

Pasta: `logs/aplicacao-YYYY-MM-DD.txt`

Exemplo de linha:
```
2024-01-15 14:23:45.123 +00:00 [INF] Cidades importadas com sucesso do arquivo: cidade.csv
2024-01-15 14:24:10.456 +00:00 [ERR] Erro ao processar arquivo: dados incompletos
```

---

## ⚡ POSSÍVEIS ERROS

| Mensagem | Solução |
|----------|---------|
| `A porta 5001 já está em uso` | Feche outro Visual Studio ou rode em porta diferente |
| `SSL certificate not trusted` | Normal em dev, pode ignorar ou usar flag `-k` no curl |
| `Arquivo não contém dados válidos` | CSV vazio ou sem linhas após header |
| `Encontradas X linhas com dados incompletos` | Verifique se CidadeId, Nome, Sigla, IBGEMunicipio estão preenchidos |

---

## 💡 DICAS

✅ CSV precisa ter:
```
CidadeId,Nome,Sigla,IBGEMunicipio,Latitude,Longitude
1,"Nome da Cidade",RO,123,-10.5,-60.5
```

✅ Sigla deve ter 2 caracteres (RO, SP, MG, etc.)

✅ Latitude/Longitude podem ser vazios (deixar em branco)

✅ Nomes podem ter acentos e caracteres especiais (UTF-8)

---

## 📖 Documentação Completa

- `ENDPOINTS_RESUMO.md` - Todos os endpoints
- `GUIA_TESTES_CSV.md` - Como testar passo a passo
- `RESUMO_EXECUTIVO.md` - Visão geral do projeto

---

## ✨ Você tem TUDO PRONTO!

Agora é só:
1. Abrir PowerShell na pasta do projeto
2. Rodar `dotnet run`
3. Testar em `https://localhost:5001/doc`
4. Importar seu CSV
5. Verificar os logs

**Pronto! 🎉**
