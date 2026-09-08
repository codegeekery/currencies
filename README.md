# Currencies — API + App

Projeto composto por duas partes:

- **Backend** (`.NET 10` + `EF Core` + `Swagger`) — expõe a API de moedas.
- **Frontend** (`Python` + `Flet`) — consome a API e exibe a interface.

---

## 1. Backend (.NET)

### Dependências

- [.NET 10 SDK](https://dotnet.microsoft.com/download) instalado
- Pacotes NuGet (já referenciados no `.csproj`, restaurados automaticamente):
  - `Microsoft.EntityFrameworkCore.Sqlite` (10.0.11)
  - `Swashbuckle.AspNetCore` (10.2.3)

Verifique se o SDK está instalado:

```bash
dotnet --version
```

Deve retornar `10.x.x` ou superior.

### Como iniciar

Na pasta do projeto backend (onde está o `.csproj`):

```bash
# 1. Restaurar os pacotes NuGet
dotnet restore

# 2. Iniciar a API
dotnet run
```

Ao rodar `dotnet run`, o console mostrará a porta em que a API subiu, por exemplo:

```
Now listening on: http://localhost:5198
```

> **Importante:** copie essa porta — ela precisa ser configurada no frontend (veja a seção 2).
> Você também pode encontrá-la em `Properties/launchSettings.json`.

### Documentação da API (Swagger)

Com o projeto rodando, acesse:

```
http://localhost:{PORTA}/swagger
```

para ver e testar todos os endpoints disponíveis.

---

## 2. Frontend (Python / Flet)

### Dependências

- Python 3.10+
- Bibliotecas:
  - `flet`
  - `httpx`

Instale com:

```bash
pip install flet httpx ou pip install -r requirements.txt
```

### Configuração

Antes de rodar, abra `main.py` e ajuste a constante `BASE_URL` com a porta real do backend (obtida no passo anterior):

```python
BASE_URL = "http://localhost:5198"  # <-- trocar pela porta real do dotnet run
```

### Como iniciar

```bash
python main.py
```

Isso abrirá a janela do app Flet, já conectada à API.

---

## 3. Ordem de execução

1. Suba primeiro o **backend** (`dotnet run`) e confirme a porta exibida no console.
2. Atualize o `BASE_URL` no `main.py` com essa porta.
3. Rode o **frontend** (`python main.py`).

Se o frontend exibir erros de conexão, verifique se o backend está rodando e se a porta em `BASE_URL` está correta.