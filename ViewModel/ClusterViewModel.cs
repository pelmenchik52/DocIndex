using KURSOVA.Model;
using KURSOVA.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel; 
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KURSOVA.ViewModel
{
    // Клас ViewModel, який представляє один кластер для відображення у View.
    public class ClusterViewModel
    {
        // Ідентифікатор кластера, отриманий від Model (Cluster.ClusterId).
        public int ClusterId { get; set; }

        // Колекція DocViewModel: Містить об'єкти ViewModel для документів, що належать цьому кластеру.
        // ObservableCollection забезпечує автоматичне оновлення UI при зміні вмісту.
        public ObservableCollection<DocViewModel> Documents { get; }
            = new ObservableCollection<DocViewModel>();

        // Обчислювана властивість: Форматує назву кластера для відображення у View.
        // ClusterId + 1 використовується для відображення відліку з "1" замість "0".
        public string ClusterName => $"Cluster {ClusterId + 1} ({Documents.Count} docs.)";
    }
}
