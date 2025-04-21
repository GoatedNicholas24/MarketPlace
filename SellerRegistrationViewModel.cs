using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Greendash1;
public class OperatingHour
{
    public string Day { get; set; }
    public TimeSpan OpenTime { get; set; }
    public TimeSpan CloseTime { get; set; }
}

public class SellerRegistrationViewModel : INotifyPropertyChanged
{
    public ObservableCollection<OperatingHour> OperatingHours { get; set; }
    public ICommand OpenTermsCommand { get; }
    

    public SellerRegistrationViewModel()
    {
        OperatingHours = new ObservableCollection<OperatingHour>
        {
            new OperatingHour { Day = "Monday", OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(17, 0, 0) },
            new OperatingHour { Day = "Tuesday", OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(17, 0, 0) },
            new OperatingHour { Day = "Wednesday", OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(17, 0, 0) },
            new OperatingHour { Day = "Thursday", OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(17, 0, 0) },
            new OperatingHour { Day = "Friday", OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(17, 0, 0) },
            new OperatingHour { Day = "Saturday", OpenTime = new TimeSpan(10, 0, 0), CloseTime = new TimeSpan(16, 0, 0) },
            new OperatingHour { Day = "Sunday", OpenTime = new TimeSpan(10, 0, 0), CloseTime = new TimeSpan(14, 0, 0) }
        };
        OpenTermsCommand = new Command(OpenTerms);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private async void OpenTerms()
    {
        await Application.Current.MainPage.Navigation.PushAsync(new TermsAndConditions());
    }
}
