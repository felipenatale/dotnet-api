using System.Linq;
using Alura.ListaLeitura.Persistencia;
using Alura.ListaLeitura.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace Alura.ListaLeitura.WebApp.Controllers
{
    [Authorize]
    public class LivroController : Controller
    {
        private const string UriString = "http://localhost:6000/api/";
        private readonly IRepository<Livro> _repo;

        public LivroController(IRepository<Livro> repository)
        {
            _repo = repository;
        }

        [HttpGet]
        public IActionResult Novo()
        {
            return View(new LivroUpload());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Novo(LivroUpload model)
        {
            if (ModelState.IsValid)
            {
                _repo.Incluir(model.ToLivro());
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ImagemCapa(int id)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new System.Uri(UriString);
            HttpResponseMessage resposta = await httpClient.GetAsync($"ListasLeitura/{id}/capa");
            resposta.EnsureSuccessStatusCode();

            byte[] img = await resposta.Content.ReadAsByteArrayAsync();

            if (img != null)
            {
                return File(img, "image/png");
            }
            return File("~/images/capas/capa-vazia.png", "image/png");
        }

        //[HttpGet]
        //public IActionResult Detalhes(int id)
        //{
        //    var model = _repo.Find(id);
        //    if (model == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(model.ToModel());
        //}

        [HttpGet]
        public async Task<IActionResult> Detalhes(int id)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new System.Uri(UriString);
            HttpResponseMessage resposta = await httpClient.GetAsync($"livros/{id}");
            resposta.EnsureSuccessStatusCode();

            var model = await resposta.Content.ReadAsAsync<LivroApi>();

            if (model == null)
            {
                return NotFound();
            }
            return View(model.ToUpload());
        }

        [HttpGet]
        public IActionResult DetalhesJson_(int id)
        {
            var model = _repo.Find(id);
            if (model == null)
            {
                return NotFound();
            }
            return Json(model.ToModel());
        }

        public ActionResult<LivroUpload> DetalhesJson(int id) 
        {
            var model = _repo.Find(id);
            if(model == null)
            {
                return NotFound();
            }
            return model.ToModel();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Detalhes(LivroUpload model)
        {
            if (ModelState.IsValid)
            {
                var livro = model.ToLivro();
                if (model.Capa == null)
                {
                    livro.ImagemCapa = _repo.All
                        .Where(l => l.Id == livro.Id)
                        .Select(l => l.ImagemCapa)
                        .FirstOrDefault();
                }
                _repo.Alterar(livro);
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remover(int id)
        {
            var model = _repo.Find(id);
            if (model == null)
            {
                return NotFound();
            }
            _repo.Excluir(model);
            return RedirectToAction("Index", "Home");
        }
    }
}