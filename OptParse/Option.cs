using System;

namespace OptParse
{

    /// <summary>
    /// Class for storing objects and values 
    /// - contains shortcut, full, options and default values.
    /// 
    /// @author strnadj
    /// </summary>
    public class Option : IComparable<Option>
    {
        /// <summary>
        /// Type - default is required.
        /// </summary>
        private readonly OptionType _optionType;

        /// <summary>
        /// Short option name.
        /// </summary>
        private readonly char _shortName = 'a';

        /// <summary>
        /// Full option name.
        /// </summary>
        private readonly string _fullName;

        /// <summary>
        /// Default value string.
        /// </summary>
        private readonly string _defaultValue;

        /// <summary>
        /// Position in option list.
        /// </summary>
        public readonly int Position;

        /// <summary>
        /// Filled?! </summary>
        private bool _filled;

        /// <summary>
        /// Value. </summary>
        private string _value = "";

        /// <summary>
        /// Default option with specification of required values.
        /// </summary>
        /// <param name="shortName"> Shortcut </param>
        /// <param name="fullName"> Full name </param>
        /// <param name="defaultValue"> Default value </param>
        /// <param name="optionType"> Option optionType </param>
        /// <param name="description"> Description </param>
        /// <param name="valueRequired"> Value required? </param>
        public Option(char shortName, string fullName, string defaultValue, OptionType optionType, string description, bool valueRequired)
        {
            _shortName = shortName;
            _fullName = fullName;
            _defaultValue = defaultValue;
            _optionType = optionType;
            Position = -1;
            Description = description;
            ValueRequired = valueRequired;
        }

        /// <summary>
        /// Default OPTIONAL option constructor.
        /// </summary>
        /// <param name="shortName"> Shortcut </param>
        /// <param name="fullName"> Full name </param>
        /// <param name="defaultValue"> Default value </param>
        /// <param name="optionType"> Option optionType </param>
        /// <param name="description"> Description </param>
        public Option(char shortName, string fullName, string defaultValue, OptionType optionType, string description)
            : this(shortName, fullName, defaultValue, optionType, description, false)
        {
        }

        /// <summary>
        /// Path or expression option.
        /// </summary>
        /// <param name="fullName"> Full name </param>
        /// <param name="optionType"> Option optionType </param>
        /// <param name="defaultValue"> Default value </param>
        /// <param name="position"> Position </param>
        /// <param name="description"> Description </param>
        public Option(string fullName, OptionType optionType, string defaultValue, int position, string description)
        {
            _fullName = fullName;
            _defaultValue = defaultValue;
            Position = position;
            Description = description;
            _optionType = optionType;
        }

        /// <summary>
        /// Is value required?
        /// </summary>
        public virtual bool ValueRequired { get; }

        /// <summary>
        /// Is option filled? </summary>
        /// <returns> True if its </returns>
        public virtual bool Filled => _filled;

        /// <summary>
        /// Set option as filled.
        /// </summary>
        public virtual void SetFilled()
        {
            _filled = true;
        }

        /// <summary>
        /// Return value (if is not filled return default value!)
        /// </summary>
        /// <returns> Value or default value </returns>
        public virtual string Value()
        {
            if (!Filled)
            {
                return _defaultValue;
            }
            return _value;
        }

        /// <summary>
        /// Return actual value (always return value no default!).
        /// </summary>
        /// <returns> String of actual value. </returns>
        public virtual string RawValue
        {
            get => _value;
            set => _value = value;
        }


        /// <summary>
        /// Get optionType of option. </summary>
        /// <returns> Option optionType </returns>
        public virtual OptionType OptionType => _optionType;

        /// <summary>
        /// Return option shortcut. 
        /// </summary>
        public virtual char ShortName => _shortName;

        /// <summary>
        /// Return full name of option. 
        /// </summary>
        public virtual string FullName => _fullName;

        /// <summary>
        /// Get description.
        /// </summary>
        public virtual string Description { get; }

        /// <summary>
        /// Is option required? 
        /// </summary>
        public virtual bool Required => _optionType == OptionType.Required;

        /// <summary>
        /// Compare options for uniqueness in set.
        /// </summary>
        /// <returns> True if options are same  </returns>
        public virtual bool Equals(Option aOption)
        {
            if (_shortName == aOption.ShortName || _fullName == aOption.FullName)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Compare options.
        /// </summary>
        /// <returns> Messure of comparing  </returns>
        public virtual int CompareTo(Option o)
        {
            // First required than optional, first short name
            if (_optionType != o.OptionType)
            {
                return _optionType == OptionType.Required ? 1 : -1;
            }
            if (_shortName != o.ShortName)
            {
                return _shortName - o.ShortName;
            }
            return String.Compare(_fullName, o.FullName, StringComparison.Ordinal);
        }
    }
}
