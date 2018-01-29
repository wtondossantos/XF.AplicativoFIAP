using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using XF.AplicativoFIAP.Model;

namespace XF.AplicativoFIAP.ViewModel
{
    public class ProfessorViewModel : INotifyPropertyChanged
    {
        #region Propriedades
        static ProfessorViewModel instancia = new ProfessorViewModel();
        public static ProfessorViewModel Instancia
        {
            get { return instancia; }
            private set { instancia = value; }
        }

        public Professor ProfessorModel { get; set; }

        private Professor selecionado;
        public Professor Selecionado
        {
            get { return selecionado; }
            set
            {
                selecionado = value as Professor;
                EventPropertyChanged();
            }
        }

        private string pesquisaPorNome;
        public string PesquisaPorNome
        {
            get { return pesquisaPorNome; }
            set
            {
                if (value == pesquisaPorNome) return;

                pesquisaPorNome = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PesquisaPorNome)));
                AplicarFiltro();
            }
        }

        public List<Professor> CopiaListaProfessores;
        public ObservableCollection<Professor> Professores { get; set; } = new ObservableCollection<Professor>();

        // UI Events
        public OnAdicionarProfessorCMD OnAdicionarProfessorCMD { get; }
        public OnEditarProfessorCMD OnEditarProfessorCMD { get; }
        public OnDeleteProfessorCMD OnDeleteProfessorCMD { get; }
        public ICommand OnSairCMD { get; private set; }
        public ICommand OnNovoCMD { get; private set; }

        #endregion

        public ProfessorViewModel()
        {
            OnAdicionarProfessorCMD = new OnAdicionarProfessorCMD(this);
            OnEditarProfessorCMD = new OnEditarProfessorCMD(this);
            OnDeleteProfessorCMD = new OnDeleteProfessorCMD(this);
            OnSairCMD = new Command(OnSair);
            OnNovoCMD = new Command(OnNovo);

            CopiaListaProfessores = new List<Professor>();
        }

        public async Task Carregar()
        {
            await ProfessorRepository.GetProfessoresSqlAzureAsync().ContinueWith(retorno =>
            {
                CopiaListaProfessores = retorno.Result.ToList();
            });
            AplicarFiltro();
        }

        public void AplicarFiltro()
        {
            if (pesquisaPorNome == null)
                pesquisaPorNome = "";

            var resultado = CopiaListaProfessores.Where(n => n.Nome.ToLowerInvariant()
                                .Contains(PesquisaPorNome.ToLowerInvariant().Trim())).ToList();

            var removerDaLista = Professores.Except(resultado).ToList();
            foreach (var item in removerDaLista)
            {
                Professores.Remove(item);
            }

            for (int index = 0; index < resultado.Count; index++)
            {
                var item = resultado[index];
                if (index + 1 > Professores.Count || !Professores[index].Equals(item))
                    Professores.Insert(index, item);
            }
        }

        public async void Adicionar(Professor paramProfessor)
        {
            if ((paramProfessor == null) || (string.IsNullOrWhiteSpace(paramProfessor.Nome)))
                await App.Current.MainPage.DisplayAlert("Atenção", "O campo nome é obrigatório", "OK");
            else if (await ProfessorRepository.PostProfessorSqlAzureAsync(paramProfessor))
                await App.Current.MainPage.Navigation.PopAsync();
            else
                await App.Current.MainPage.DisplayAlert("Falhou", "Desculpe, ocorreu um erro inesperado =(", "OK");
        }

        public async void Editar()
        {
            await App.Current.MainPage.Navigation.PushAsync(
                new View.NovoProfessorView() { BindingContext = Instancia });
        }

        public async void Remover()
        {
            if (await App.Current.MainPage.DisplayAlert("Atenção?",
                string.Format("Tem certeza que deseja remover o {0}?", Selecionado.Nome), "Sim", "Não"))
            {
                if (await ProfessorRepository.DeleteProfessorSqlAzureAsync(Selecionado.Id.ToString()))
                {
                    CopiaListaProfessores.Remove(Selecionado);
                    await Carregar();
                }
                else
                    await App.Current.MainPage.DisplayAlert(
                            "Falhou", "Desculpe, ocorreu um erro inesperado =(", "OK");
            }
        }

        private void OnNovo()
        {
            Instancia.Selecionado = new Model.Professor();
            App.Current.MainPage.Navigation.PushAsync(
                new View.NovoProfessorView() { BindingContext = Instancia });
        }

        private async void OnSair()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void EventPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class OnAdicionarProfessorCMD : ICommand
    {
        private ProfessorViewModel professorVM;
        public OnAdicionarProfessorCMD(ProfessorViewModel paramVM)
        {
            professorVM = paramVM;
        }
        public event EventHandler CanExecuteChanged;
        public void AdicionarCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter)
        {
            professorVM.Adicionar(parameter as Professor);
        }
    }

    public class OnEditarProfessorCMD : ICommand
    {
        private ProfessorViewModel professorVM;
        public OnEditarProfessorCMD(ProfessorViewModel paramVM)
        {
            professorVM = paramVM;
        }
        public event EventHandler CanExecuteChanged;
        public void EditarCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        public bool CanExecute(object parameter) => (parameter != null);
        public void Execute(object parameter)
        {
            ProfessorViewModel.Instancia.Selecionado = parameter as Professor;
            professorVM.Editar();
        }
    }

    public class OnDeleteProfessorCMD : ICommand
    {
        private ProfessorViewModel professorVM;
        public OnDeleteProfessorCMD(ProfessorViewModel paramVM)
        {
            professorVM = paramVM;
        }
        public event EventHandler CanExecuteChanged;
        public void DeleteCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        public bool CanExecute(object parameter) => (parameter != null);
        public void Execute(object parameter)
        {
            ProfessorViewModel.Instancia.Selecionado = parameter as Professor;
            professorVM.Remover();
        }
    }
}
