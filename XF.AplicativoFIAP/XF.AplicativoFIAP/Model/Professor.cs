using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace XF.AplicativoFIAP.Model
{
    public class Professor
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Titulo { get; set; }
    }
}
