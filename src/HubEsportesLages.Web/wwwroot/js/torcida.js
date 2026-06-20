// Hub Esportes Lages — cliente da interação da torcida (consome a API REST da Fase 1).
// Identidade do torcedor: GUID em localStorage, enviado no header X-Torcedor-Id.
(function () {
    "use strict";

    var raiz = document.getElementById("torcida");
    if (!raiz) return;

    var slug = raiz.dataset.slug;
    var aoVivo = raiz.dataset.aoVivo === "true";
    var POLL_MS = 4000;

    var equipes = [
        { id: raiz.dataset.equipeCasaId, nome: raiz.dataset.equipeCasaNome },
        { id: raiz.dataset.equipeVisitanteId, nome: raiz.dataset.equipeVisitanteNome }
    ].filter(function (e) { return e.id; });

    var elAviso = document.getElementById("torcidaAviso");
    var elMvp = document.getElementById("mvpLista");
    var elEnquete = document.getElementById("enquete");
    var elFavoritos = document.getElementById("favoritos");
    var elMural = document.getElementById("mural");
    var muralForm = document.getElementById("muralForm");
    var muralTexto = document.getElementById("muralTexto");
    var muralBtn = document.getElementById("muralBtn");

    var votandoMvp = false;
    var votandoEnquete = false;
    var enviando = false;
    var timer = null;

    // ───────────────────────────────────────────────── identidade
    function getTorcedorId() {
        var id = localStorage.getItem("hub.torcedorId");
        if (!id) {
            id = (window.crypto && crypto.randomUUID) ? crypto.randomUUID() : gerarGuid();
            localStorage.setItem("hub.torcedorId", id);
        }
        return id;
    }

    function gerarGuid() {
        return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0;
            var v = c === "x" ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    // ───────────────────────────────────────────────── HTTP
    function api(metodo, caminho, corpo) {
        var opcoes = {
            method: metodo,
            headers: { "X-Torcedor-Id": getTorcedorId() }
        };
        if (corpo !== undefined) {
            opcoes.headers["Content-Type"] = "application/json";
            opcoes.body = JSON.stringify(corpo);
        }
        return fetch(caminho, opcoes);
    }

    function traduzErro(status) {
        if (status === 409) return "A interação só está disponível com o jogo ao vivo.";
        if (status === 429) return "Aguarde alguns segundos antes de tentar novamente.";
        if (status === 404) return "Evento não encontrado.";
        return "Não foi possível concluir a ação. Tente novamente.";
    }

    function aviso(mensagem) {
        if (!elAviso) return;
        if (!mensagem) { elAviso.style.display = "none"; return; }
        elAviso.textContent = mensagem;
        elAviso.style.display = "block";
    }

    function escapar(texto) {
        var div = document.createElement("div");
        div.textContent = texto == null ? "" : String(texto);
        return div.innerHTML;
    }

    // ───────────────────────────────────────────────── carregar estado
    function carregar() {
        api("GET", "/api/eventos/" + encodeURIComponent(slug) + "/torcida")
            .then(function (r) {
                if (!r.ok) throw new Error("estado " + r.status);
                return r.json();
            })
            .then(renderEstado)
            .catch(function () { /* silencioso durante polling */ });
    }

    function renderEstado(e) {
        renderMvp(e.mvp);
        renderEnquete(e.enquete);
        renderFavoritos(e.favoritado);
        renderMural(e.mensagens);
    }

    // ───────────────────────────────────────────────── MVP
    function renderMvp(mvp) {
        if (!mvp || !mvp.candidatos || mvp.candidatos.length === 0) {
            elMvp.innerHTML = '<p class="muted">Escalação ainda não definida.</p>';
            return;
        }
        var meu = mvp.meuVotoJogadorId;
        var votou = meu != null;
        elMvp.innerHTML = mvp.candidatos.map(function (c) {
            var ativo = meu === c.jogadorEventoId ? " torcida-opcao--ativa" : "";
            var equipe = c.equipe ? '<span class="muted"> · ' + escapar(c.equipe) + "</span>" : "";
            return '<button type="button" class="torcida-opcao' + ativo + '" ' +
                (votou ? "disabled" : "") +
                ' data-jogador="' + c.jogadorEventoId + '">' +
                '<span>' + escapar(c.nome) + equipe + "</span>" +
                '<span class="torcida-opcao__votos">' + c.votos + "</span>" +
                "</button>";
        }).join("");

        if (!votou) {
            elMvp.querySelectorAll("[data-jogador]").forEach(function (btn) {
                btn.addEventListener("click", function () { votarMvp(parseInt(btn.dataset.jogador, 10)); });
            });
        }
    }

    function votarMvp(jogadorEventoId) {
        if (votandoMvp) return;
        votandoMvp = true;
        aviso("");
        api("POST", "/api/eventos/" + encodeURIComponent(slug) + "/torcida/mvp", { jogadorEventoId: jogadorEventoId })
            .then(function (r) {
                if (!r.ok) { aviso(traduzErro(r.status)); return null; }
                return r.json();
            })
            .then(function (e) { if (e) renderEstado(e); })
            .finally(function () { votandoMvp = false; });
    }

    // ───────────────────────────────────────────────── Enquete
    function renderEnquete(enquete) {
        if (!enquete) {
            elEnquete.innerHTML = '<p class="muted">Nenhuma enquete ativa no momento.</p>';
            return;
        }
        var votou = enquete.minhaOpcaoId != null;
        var html = "<p style=\"font-weight:600; margin:0 0 14px;\">" + escapar(enquete.pergunta) + "</p>";
        html += enquete.opcoes.map(function (o) {
            var ativo = enquete.minhaOpcaoId === o.id ? " torcida-opcao--ativa" : "";
            return '<button type="button" class="torcida-opcao' + ativo + '" ' +
                (votou ? "disabled" : "") +
                ' data-opcao="' + o.id + '">' +
                '<span>' + escapar(o.texto) + "</span>" +
                '<span class="torcida-opcao__votos">' + o.percentual + "%</span>" +
                '<span class="torcida-barra"><span class="torcida-barra__fill" style="width:' + o.percentual + '%"></span></span>' +
                "</button>";
        }).join("");
        elEnquete.innerHTML = html;

        if (!votou) {
            elEnquete.querySelectorAll("[data-opcao]").forEach(function (btn) {
                btn.addEventListener("click", function () { votarEnquete(enquete.id, parseInt(btn.dataset.opcao, 10)); });
            });
        }
    }

    function votarEnquete(enqueteId, opcaoId) {
        if (votandoEnquete) return;
        votandoEnquete = true;
        aviso("");
        api("POST", "/api/eventos/" + encodeURIComponent(slug) + "/torcida/enquete/" + enqueteId + "/voto", { opcaoId: opcaoId })
            .then(function (r) {
                if (!r.ok) { aviso(traduzErro(r.status)); return null; }
                return r.json();
            })
            .then(function (e) { if (e) renderEstado(e); })
            .finally(function () { votandoEnquete = false; });
    }

    // ───────────────────────────────────────────────── Favoritos
    function renderFavoritos(favoritado) {
        if (equipes.length === 0) {
            elFavoritos.innerHTML = '<p class="muted">Sem equipes neste evento.</p>';
            return;
        }
        elFavoritos.innerHTML = equipes.map(function (eq) {
            var ativo = favoritado ? " torcida-opcao--ativa" : "";
            var rotulo = favoritado ? "✓ Favoritada" : "❤ Favoritar";
            return '<button type="button" class="torcida-opcao' + ativo + '" ' +
                'data-equipe="' + eq.id + '" data-fav="' + (favoritado ? "1" : "0") + '">' +
                "<span>" + escapar(eq.nome) + "</span>" +
                '<span class="torcida-opcao__votos">' + rotulo + "</span>" +
                "</button>";
        }).join("");

        elFavoritos.querySelectorAll("[data-equipe]").forEach(function (btn) {
            btn.addEventListener("click", function () {
                alternarFavorito(parseInt(btn.dataset.equipe, 10), btn.dataset.fav === "1");
            });
        });
    }

    function alternarFavorito(equipeId, jaFavorito) {
        aviso("");
        api(jaFavorito ? "DELETE" : "POST", "/api/favoritos/equipes/" + equipeId)
            .then(function (r) {
                if (!r.ok) { aviso(traduzErro(r.status)); return; }
                renderFavoritos(!jaFavorito);
            });
    }

    // ───────────────────────────────────────────────── Mural
    function renderMural(mensagens) {
        if (!mensagens || mensagens.length === 0) {
            elMural.innerHTML = '<p class="muted">Seja o primeiro a torcer aqui.</p>';
            return;
        }
        elMural.innerHTML = mensagens.map(function (m) {
            return '<div class="torcida-msg">' +
                '<span class="torcida-msg__autor">' + escapar(m.autor) + "</span>" +
                '<span class="torcida-msg__texto">' + escapar(m.texto) + "</span>" +
                "</div>";
        }).join("");
    }

    function enviarMensagem(ev) {
        ev.preventDefault();
        if (enviando) return;
        var texto = (muralTexto.value || "").trim();
        if (!texto) return;
        enviando = true;
        muralBtn.disabled = true;
        aviso("");
        api("POST", "/api/eventos/" + encodeURIComponent(slug) + "/torcida/mensagens", { texto: texto })
            .then(function (r) {
                if (!r.ok) { aviso(traduzErro(r.status)); return; }
                muralTexto.value = "";
                carregar();
            })
            .finally(function () {
                enviando = false;
                muralBtn.disabled = false;
            });
    }

    // ───────────────────────────────────────────────── init
    if (muralForm) muralForm.addEventListener("submit", enviarMensagem);

    if (!aoVivo) {
        // Modo leitura (Agendado/Encerrado): sem envio de mensagens.
        if (muralTexto) { muralTexto.disabled = true; muralTexto.placeholder = "Mural disponível durante o jogo."; }
        if (muralBtn) muralBtn.disabled = true;
    }

    carregar();

    if (aoVivo) {
        timer = setInterval(carregar, POLL_MS);
        window.addEventListener("beforeunload", function () { if (timer) clearInterval(timer); });
    }
})();
