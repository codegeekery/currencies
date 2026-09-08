import flet as ft
import requests

API_URL = "http://localhost:5198"


def main(page: ft.Page):
    page.title = "Currencies API Client"
    page.vertical_alignment = ft.MainAxisAlignment.START
    page.horizontal_alignment = ft.CrossAxisAlignment.CENTER
    page.padding = 20
    page.theme_mode = ft.ThemeMode.LIGHT

    # --- PAINEL 1: CONVERSÃO ---
    txt_from = ft.TextField(
        label="Moeda Origem (ex: USD)", width=150, value="USD"
    )
    txt_to = ft.TextField(
        label="Moeda Destino (ex: EUR)", width=150, value="EUR"
    )
    txt_amount = ft.TextField(label="Valor", width=150, value="100")
    txt_resultado = ft.Text(
        value="", size=16, weight=ft.FontWeight.BOLD, color=ft.Colors.GREEN_700
    )

    def converter_moeda(e):
        try:
            url = f"{API_URL}/api/convert?from={txt_from.value}&to={txt_to.value}&amount={txt_amount.value}"
            response = requests.get(url)
            if response.status_code == 200:
                data = response.json()
                txt_resultado.value = f"Resultado: {data['result']} {data['to']} (Taxa: {data['rate']})"
            else:
                txt_resultado.value = f"Erro: {response.text}"
        except Exception as ex:
            txt_resultado.value = f"Erro de conexão: {ex}"
        page.update()

    btn_converter = ft.ElevatedButton("Converter", on_click=converter_moeda)

    painel_conversao = ft.Container(
        content=ft.Column(
            [
                ft.Text(
                    "Conversão de Moedas",
                    size=20,
                    weight=ft.FontWeight.BOLD,
                ),
                ft.Row(
                    [txt_from, txt_to, txt_amount],
                    alignment=ft.MainAxisAlignment.CENTER,
                ),
                btn_converter,
                txt_resultado,
            ],
            horizontal_alignment=ft.CrossAxisAlignment.CENTER,
            spacing=20,
        ),
        padding=20,
    )

    # --- PAINEL 2: LISTAR MOEDAS ---
    lista_moedas = ft.ListView(expand=1, spacing=5, padding=10)

    def carregar_moedas(e=None):
        lista_moedas.controls.clear()
        try:
            response = requests.get(f"{API_URL}/api/currencies")
            if response.status_code == 200:
                moedas = response.json()
                for m in moedas:
                    lista_moedas.controls.append(
                        ft.ListTile(
                            leading=ft.Icon(ft.Icons.MONETIZATION_ON),
                            title=ft.Text(f"{m['code']} - {m['name']}"),
                        )
                    )
            else:
                lista_moedas.controls.append(
                    ft.Text(f"Erro ao carregar: {response.text}")
                )
        except Exception as ex:
            lista_moedas.controls.append(
                ft.Text(f"Erro de conexão com a API: {ex}")
            )
        page.update()

    btn_atualizar_moedas = ft.ElevatedButton(
        "Atualizar Moedas", on_click=carregar_moedas
    )

    painel_moedas = ft.Container(
        content=ft.Column(
            [
                ft.Text(
                    "Moedas Suportadas", size=20, weight=ft.FontWeight.BOLD
                ),
                btn_atualizar_moedas,
                ft.Container(
                    content=lista_moedas,
                    height=400,
                    border_radius=10,
                ),
            ],
            horizontal_alignment=ft.CrossAxisAlignment.CENTER,
            spacing=15,
        ),
        padding=20,
    )

    # --- PAINEL 3: TAXAS POR DATA ---
    txt_data = ft.TextField(
        label="Data (yyyy-MM-dd)", width=200, value="2026-01-10"
    )
    lista_taxas = ft.ListView(expand=1, spacing=5, padding=10)

    def consultar_taxas(e):
        lista_taxas.controls.clear()
        try:
            url = f"{API_URL}/api/rates/{txt_data.value}"
            response = requests.get(url)
            if response.status_code == 200:
                data = response.json()
                rates = data.get("rates", {})
                for target, val in rates.items():
                    lista_taxas.controls.append(
                        ft.Text(f"1 EUR = {val} {target}")
                    )
            else:
                lista_taxas.controls.append(
                    ft.Text(f"Erro: {response.text}")
                )
        except Exception as ex:
            lista_taxas.controls.append(ft.Text(f"Erro de conexão: {ex}"))
        page.update()

    btn_consultar_taxas = ft.ElevatedButton(
        "Consultar Taxas", on_click=consultar_taxas
    )

    painel_taxas = ft.Container(
        content=ft.Column(
            [
                ft.Text(
                    "Taxas por Data (Base EUR)",
                    size=20,
                    weight=ft.FontWeight.BOLD,
                ),
                ft.Row(
                    [txt_data, btn_consultar_taxas],
                    alignment=ft.MainAxisAlignment.CENTER,
                ),
                ft.Container(
                    content=lista_taxas,
                    height=350,
                    border_radius=10,
                ),
            ],
            horizontal_alignment=ft.CrossAxisAlignment.CENTER,
            spacing=15,
        ),
        padding=20,
    )

    # --- NAVEGAÇÃO DINÂMICA ---
    conteudo_container = ft.Container(content=painel_conversao, expand=1)

    def mudar_painel(novo_painel):
        conteudo_container.content = novo_painel
        page.update()

    nav_bar = ft.Row(
        [
            ft.ElevatedButton(
                "Converter", on_click=lambda e: mudar_painel(painel_conversao)
            ),
            ft.ElevatedButton(
                "Moedas", on_click=lambda e: mudar_painel(painel_moedas)
            ),
            ft.ElevatedButton(
                "Taxas por Data", on_click=lambda e: mudar_painel(painel_taxas)
            ),
        ],
        alignment=ft.MainAxisAlignment.CENTER,
        spacing=10,
    )

    page.add(
        ft.Text(
            "Currencies Dashboard", size=24, weight=ft.FontWeight.BOLD
        ),
        nav_bar,
        conteudo_container,
    )

    carregar_moedas()


ft.app(target=main)