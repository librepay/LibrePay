using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LibrePay.Models
{
    public class Pinpad : INotifyPropertyChanged
    {
        private string _value;
        private decimal _valueDecimal;

        public string Value
        {
            get => _value;
            set => SetValueFromString(value);
        }

        public string Format { get; }

        public decimal ValueDecimal
        {
            get => _valueDecimal;
            private set => SetProperty(ref _valueDecimal, value);
        }

        public CultureInfo Culture { get; }

        public ushort DecimalDigits { get; }

        public ushort MaxLength { get; }

        public Pinpad(byte decimalDigits, byte maxLength, CultureInfo cultureInfo)
        {
            Format = $"N{decimalDigits}";
            Culture = cultureInfo;
            DecimalDigits = decimalDigits;
            MaxLength = maxLength;

            Value = "0";
        }

        #region INotifyPropertyChanged

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName] string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);

            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public void DeleteLastNumber()
        {
            var s = GetValueInListWithoutDecimalSeparator();

            s = s.Take(s.Count - 1).ToList();

            AddDecimalSeparator(s);

            SetValueFromListOfChars(s);
        }

        public void AppendNumber(char number)
        {
            var s = GetValueInListWithoutDecimalSeparator();

            if (s.Count <= MaxLength)
                s.Add(number);

            AddDecimalSeparator(s);

            SetValueFromListOfChars(s);
        }

        private void AddDecimalSeparator(List<char> s)
        {
            s.Insert(s.Count - DecimalDigits, Culture.NumberFormat.CurrencyDecimalSeparator[0]);
        }

        private void SetValueFromListOfChars(List<char> value)
        {
            ValueDecimal = decimal.Parse(
                new string(value.ToArray())
                , Culture
            );
            SetProperty(ref _value, ValueDecimal.ToString(Format), nameof(Value));
        }

        private void SetValueFromString(string value)
        {
            ValueDecimal = decimal.Parse(value ?? "0", Culture);
            SetProperty(ref _value, ValueDecimal.ToString(Format), nameof(Value));
        }

        private List<char> GetValueInListWithoutDecimalSeparator()
        {
            return ValueDecimal.ToString(Format, Culture)
                .Replace(Culture.NumberFormat.CurrencyDecimalSeparator, string.Empty)
                .ToList();
        }
    }
}
