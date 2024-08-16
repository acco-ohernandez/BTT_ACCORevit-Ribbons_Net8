using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitRibbon_MainSourceCode_Resources
{
    public class ViewTemplateData : INotifyPropertyChanged
    {
        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        private string templateName;
        public string TemplateName
        {
            get { return templateName; }
            set
            {
                if (templateName != value)
                {
                    templateName = value;
                    OnPropertyChanged(nameof(TemplateName));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ViewTemplateData(string templateName, bool isSelected)
        {
            TemplateName = templateName;
            IsSelected = isSelected;
        }
    }
}
