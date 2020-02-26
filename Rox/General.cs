using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Markup;

namespace Rox
{
  public class EnumBindingSourceExtension : MarkupExtension
  {
    private Type _enumType;
    public Type EnumType
    {
      get { return this._enumType; }
      set
      {
        if (value != this._enumType)
        {
          if (null != value)
          {
            Type enumType = Nullable.GetUnderlyingType(value) ?? value;
            if (!enumType.IsEnum) { throw new ArgumentException("Type must be for an Enum."); }
          }
          this._enumType = value;
        }
      }
    }
    public EnumBindingSourceExtension() { }
    public EnumBindingSourceExtension(Type enumType)
    {
      this.EnumType = enumType;
    }
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      if (null == this._enumType) { throw new InvalidOperationException("The EnumType must be specified."); }
      Type actualEnumType = Nullable.GetUnderlyingType(this._enumType) ?? this._enumType;
      Array enumValues = Enum.GetValues(actualEnumType);
      if (actualEnumType == this._enumType) { return enumValues; }
      Array tempArray = Array.CreateInstance(actualEnumType, enumValues.Length + 1);
      enumValues.CopyTo(tempArray, 1);
      return tempArray;
    }
  }
  public class EnumDescriptionTypeConverter : EnumConverter
  {
    public EnumDescriptionTypeConverter(Type type) : base(type) { }
    public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
    {
      if (destinationType == typeof(string))
      {
        if (value != null)
        {
          System.Reflection.FieldInfo fi = value.GetType().GetField(value.ToString());
          if (fi != null)
          {
            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return ((attributes.Length > 0) && (!String.IsNullOrEmpty(attributes[0].Description))) ? attributes[0].Description : value.ToString();
          }
        }
        return string.Empty;
      }
      return base.ConvertTo(context, culture, value, destinationType);
    }
  }
  public static class Logic
  {
    public new static bool Equals(dynamic a, dynamic b)
    {
      var aType = a.GetType();
      var bType = b.GetType();
      if (aType != bType)
      {
        // type mismatch

        if (aType == typeof(decimal) || aType == typeof(double) || aType == typeof(int) || aType == typeof(long) || aType == typeof(short) || aType == typeof(byte))
        {
          if (bType == typeof(decimal) || bType == typeof(double) || bType == typeof(int) || bType == typeof(long) || bType == typeof(short) || bType == typeof(byte))
          {
            return a == b;
          }
        }
        return false;
      }
      return a.Equals(b);
    }
    public static bool GreaterThan(dynamic a, dynamic b)
    {
      try
      {
        return a > b;
      }
      catch (Exception)
      {
        return false;
      }
    }
    public static bool LessThan(dynamic a, dynamic b)
    {
      return a < b;
    }
    public static bool GreaterThanOrEqual(dynamic a, dynamic b)
    {
      try
      {
        return a >= b;
      }
      catch (Exception)
      {
        return false;
      }
    }
    public static bool LessThanOrEqual(dynamic a, dynamic b)
    {
      try
      {
        return a <= b;
      }
      catch (Exception)
      {
        return false;
      }
    }
    public static bool NotEqual(dynamic a, dynamic b)
    {
      try
      {
        return a != b;
      }
      catch (Exception)
      {
        return false;
      }
    }
  }
  public interface INode
  {
    NodeTypes NodeType { get; }
    string Name { get; set; }
    string Description();
    Collection<INode> Items { get; set; }
    List<NodeTypes> AllowedNodes { get; }
  }
  public enum NodeTypes
  {
    General = 0,
    Mode = 1,
    Condition = 2,
    Timer = 3,
    Initialized = 4,
    Continuous = 5,
    ConditionTrue = 6,
    ConditionFalse = 7,
    ConditionTrue1 = 8,
    ConditionFalse1 = 9,
    SetVariable = 10,
    SetMode = 11,
    Return = 12,
    Alarm = 13,
    AlarmClose = 14,
  }
  [TypeConverter(typeof(EnumDescriptionTypeConverter))]
  public enum IoControllers
  {
    [Description(" ")]
    None = 0,
    [Description("Advantech")]
    Advantech = 1,
    [Description("Keyence")]
    Keyence = 2,
  }
  public class IteMode : INode
  {
    public NodeTypes NodeType { get; } = NodeTypes.Mode;
    public string Name { get; set; }
    public List<NodeTypes> AllowedNodes { get; } = new List<NodeTypes>() { };
    public Collection<INode> Items { get; set; } = new Collection<INode>();
    public string Description() { return "{ Mode } A mode can be created to easily abort a running mode and start new sequencing. Stop and Auto modes will run with Start/Stop button. No items can be added directly. Add items to Initialize or Continuous branches."; }
    public IteMode(string name)
    {
      Name = name;
    }
  }
  public class IteFirstScan : INode
  {
    public NodeTypes NodeType { get; } = NodeTypes.Initialized;
    public string Name { get; set; }
    public List<NodeTypes> AllowedNodes { get; } = Rox.MainWindow.SequenceNodes;
    public Collection<INode> Items { get; set; } = new Collection<INode>();
    public string Description() { return "{ First Scan branch } This sequence will run one time when the program is loaded and can accept any item."; }
    public IteFirstScan(string name)
    {
      Name = name;
    }
  }
  public class IteIntialize : INode
  {
    public NodeTypes NodeType { get; } = NodeTypes.Initialized;
    public string Name { get; set; }
    public List<NodeTypes> AllowedNodes { get; } = Rox.MainWindow.SequenceNodes;
    public Collection<INode> Items { get; set; } = new Collection<INode>();
    public string Description() { return "{ Initialize branch } This sequence will only run one time when the mode is first started and before the continuous branch. This node can accept any item."; }
    public IteIntialize(string name)
    {
      Name = name;
    }
  }
  public class IteContinuous : INode
  {
    public NodeTypes NodeType { get; } = NodeTypes.Continuous;
    public string Name { get; set; }
    public List<NodeTypes> AllowedNodes { get; } = Rox.MainWindow.SequenceNodes;
    public Collection<INode> Items { get; set; } = new Collection<INode>();
    public string Description() { return "{ Continuous branch } This sequence will run repeatedly after the initalize branch has completed and while the mode is active."; }
    public IteContinuous(string name)
    {
      Name = name;
    }
  }
  public class IteCondition : INode, INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    public NodeTypes NodeType { get; } = NodeTypes.Condition;
    public string Name { get; set; }
    public List<NodeTypes> AllowedNodes { get; } = new List<NodeTypes>() { };
    public Collection<INode> Items { get; set; } = new Collection<INode>();
    public string Description() { return "{ Condition } This will evaluate a statement to true or false and run the appropriate sequence. Add items to True / False branches."; }
    public IteCondition(string name)
    {
      Name = name;
    }
    public string VariableName { get; set; }
    public dynamic DesiredValue { get; set; }
    public dynamic EvalMethodText { get; set; }
    public Func<dynamic, dynamic, bool> EvalMethod { get; set; }
    private bool _eval;
    public bool Evaluation
    {
      get { return _eval; }
      set
      {
        if (value != _eval)
        {
          _eval = value;
          this.OnPropertyChanged("Evaluation");
        }

      }
    }
    protected virtual void OnPropertyChanged(string propertyName)
    {
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
  public class IteSetVar : INode, INotifyPropertyChanged
  {
    private AssignMethod _method = AssignMethod.assign;
    public AssignMethod AssignMethod
    {
      get { return _method; }
      set
      {
        if (value != _method)
        {
          _method = value;
          this.OnPropertyChanged("AssignMethod");
        }
      }
    }
    private VariableType _vartype = VariableTypes.stringType;
    public VariableType VarType
    {
      get { return _vartype; }
      set
      {
        if (value.Value != _vartype.Value)
        {
          _vartype = value;
          this.OnPropertyChanged("VarType");
        }
      }
    }
    public event PropertyChangedEventHandler PropertyChanged;
    public NodeTypes NodeType { get; } = NodeTypes.SetVariable;
    public string Name { get; set; }
    public List<NodeTypes> AllowedNodes { get; } = new List<NodeTypes>() { };
    public Collection<INode> Items { get; set; } = new Collection<INode>();
    public string Description() { return "{ Set Variable } This assigns a value to a variable. Sequence items are not allowed."; }
    public IteSetVar(string name)
    {
      Name = name;
    }
    public string VariableName { get; set; }
    public dynamic Value { get; set; }
    public dynamic _otherwise = null;
    public dynamic OtherwiseValue
    {
      get { return _otherwise; }
      set
      {
        _otherwise = (value == null || string.IsNullOrEmpty(value.ToString())) ? null : value;
      }
    }
    protected virtual void OnPropertyChanged(string propertyName)
    {
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
  public class IteSetMode : INode, INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    public NodeTypes NodeType { get; } = NodeTypes.SetMode;
    public string Name { get; set; }
    public List<NodeTypes> AllowedNodes { get; } = new List<NodeTypes>() { };
    public Collection<INode> Items { get; set; } = new Collection<INode>();
    public string Description() { return "{ Set Mode } Changes the current mode to the assigned value. Sequence items are not allowed but remaining nodes and subsequent branches will still be processed for the current iteration. Use a Return to abandon remaining sequence."; }
    public IteSetMode(string name)
    {
      Name = name;
    }
    public string ModeName { get; set; }
    //public dynamic Value { get; set; }
    protected virtual void OnPropertyChanged(string propertyName)
    {
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
  public class IteReturn : INode, INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    public NodeTypes NodeType { get; } = NodeTypes.Return;
    public string Name { get; set; }
    public List<NodeTypes> AllowedNodes { get; } = new List<NodeTypes>() { };
    public Collection<INode> Items { get; set; } = new Collection<INode>();
    public string Description() { return "{ Return } Aborts the current iteration."; }
    public IteReturn(string name)
    {
      Name = name;
    }
    protected virtual void OnPropertyChanged(string propertyName)
    {
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
  public class IteAlarm : INode, INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    public NodeTypes NodeType { get; } = NodeTypes.Alarm;
    public string Name { get; set; }
    public List<NodeTypes> AllowedNodes { get; } = new List<NodeTypes>() { };
    public Collection<INode> Items { get; set; } = new Collection<INode>();
    public string Description() { return "{ Alarm } Shows a dialog with the specified information. Title must be unique to the alarm. Prompt will show in the center of the window. Color 1 and 2 is the border color and can easily distinguish different alarms. Colors can be set to common names like red, green, darkblue, lightgray, etc. They can also be in hex format. For example #FFFF0000."; }
    public string Prompt { get; set; } = "Unknown Alarm";
    public string VariableNameOnOkClick { get; set; }
    public string VariableNameOnCancelClick { get; set; }
    public dynamic OkValue { get; set; }
    public dynamic CancelValue { get; set; }
    public string Title { get; set; } = "Unknown Alarm";
    public string Color1 { get; set; } = "Black";
    public string Color2 { get; set; } = "White";
    public IteAlarm(string name)
    {
      Name = name;
    }
    protected virtual void OnPropertyChanged(string propertyName)
    {
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
  public class IteAlarmClose : INode, INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    public NodeTypes NodeType { get; } = NodeTypes.AlarmClose;
    public string Name { get; set; }
    public List<NodeTypes> AllowedNodes { get; } = new List<NodeTypes>() { };
    public Collection<INode> Items { get; set; } = new Collection<INode>();
    public string Description() { return "{ Close Alarm } Attempts to close all active alarms with the given title."; }
    public string Prompt { get; set; } = "Close Alarm";
    public string Title { get; set; } = string.Empty;
    public IteAlarmClose(string name)
    {
      Name = name;
    }
    protected virtual void OnPropertyChanged(string propertyName)
    {
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
  public class IteTimer : INode, INotifyPropertyChanged
  {
    private double _timeElapsed;
    private double _interval;
    private bool _expired;
    private DateTime? _lastTimeCalculated;

    public event PropertyChangedEventHandler PropertyChanged;
    public NodeTypes NodeType { get; } = NodeTypes.Timer;
    public string Name { get; set; }
    public List<NodeTypes> AllowedNodes { get; } = new List<NodeTypes>() { };
    public Collection<INode> Items { get; set; } = new Collection<INode>();
    public string Description() { return "{ Timer } An accumulating millisecond timer. The timer can be running or expired and sequence items can be added to these conditions. The timer is reset if containing sequence is not executing."; }
    public IteTimer(string name)
    {
      Name = name;
    }
    public double Interval { get { return _interval; } set { if (_interval != value) { _interval = value; OnPropertyChanged("Interval"); } } }
    public double TimeElapsed { get { return _timeElapsed; } private set { if (_timeElapsed != value) { _timeElapsed = value; OnPropertyChanged("TimeElapsed"); } } }
    public DateTime LastTimeCalculated { get { return _lastTimeCalculated ?? DateTime.Now; } private set { _lastTimeCalculated = value; } }
    public bool Expired
    {
      get { return _expired; }
      private set { if (_expired != value) { _expired = value; OnPropertyChanged("Expired"); } }
    }
    public void UpdateTime()
    {
      DateTime now = DateTime.Now;
      double timeFromLastCheck = (now - LastTimeCalculated).TotalMilliseconds;
      LastTimeCalculated = now;
      TimeElapsed += timeFromLastCheck;
      Expired = _timeElapsed >= _interval;
    }
    public void Stop() { _lastTimeCalculated = null; TimeElapsed = 0; Expired = false; }
    protected virtual void OnPropertyChanged(string propertyName)
    {
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
  public class IteTrue : INode
  {
    public NodeTypes NodeType { get; } = NodeTypes.ConditionTrue;
    public string Name { get; set; }
    public List<NodeTypes> AllowedNodes { get; } = Rox.MainWindow.SequenceNodes;
    public Collection<INode> Items { get; set; } = new Collection<INode>();
    public string Description() { return "{ Condition True branch } This sequence will run while the condition is true."; }
    public IteTrue(string name)
    {
      Name = name;
    }
  }
  public class IteTrue1 : INode
  {
    public NodeTypes NodeType { get; } = NodeTypes.ConditionTrue1;
    public string Name { get; set; }
    public List<NodeTypes> AllowedNodes { get; } = Rox.MainWindow.SequenceNodes;
    public Collection<INode> Items { get; set; } = new Collection<INode>();
    public string Description() { return "{ Condition True Initialized branch } This sequence will run when the condition first becomes true."; }
    public IteTrue1(string name)
    {
      Name = name;
    }
  }
  public class IteFalse : INode
  {
    public NodeTypes NodeType { get; } = NodeTypes.ConditionFalse;
    public string Name { get; set; }
    public List<NodeTypes> AllowedNodes { get; } = Rox.MainWindow.SequenceNodes;
    public Collection<INode> Items { get; set; } = new Collection<INode>();
    public string Description() { return "{ Condition False branch } This sequence will run while the condition is not true."; }
    public IteFalse(string name)
    {
      Name = name;
    }
  }
  public class IteFalse1 : INode
  {
    public NodeTypes NodeType { get; } = NodeTypes.ConditionFalse1;
    public string Name { get; set; }
    public List<NodeTypes> AllowedNodes { get; } = Rox.MainWindow.SequenceNodes;
    public Collection<INode> Items { get; set; } = new Collection<INode>();
    public string Description() { return "{ Condition False Initialized branch } This sequence will run when the condition first becomes false."; }
    public IteFalse1(string name)
    {
      Name = name;
    }
  }
  public class IteNodeViewModel : INotifyPropertyChanged
  {
    public INode Node { get; set; }
    public event PropertyChangedEventHandler PropertyChanged;
    private bool _isSelected;
    private bool _isExpanded = true;
    public IteNodeViewModel Parent { get; set; }
    public Collection<IteNodeViewModel> Items { get; set; }
    public IteNodeViewModel(INode node) : this(node, null) { }
    public IteNodeViewModel(INode node, IteNodeViewModel parent)
    {
      Node = node;
      Parent = parent;
      Items = new Collection<IteNodeViewModel>();
      foreach (var item in node.Items)
      {
        switch (item.NodeType)
        {
          case NodeTypes.General:
            //Items.Add(new IteNodeViewModel(item));
            break;
          case NodeTypes.Mode:
            Items.Add(new IteMODE_VM(item));
            break;
          case NodeTypes.Condition:
            Items.Add(new IteCONDITION_VM(item));
            break;
          case NodeTypes.Timer:
            Items.Add(new IteTIMER_VM(item));
            break;
          case NodeTypes.Initialized:
            Items.Add(new IteFIRST_VM(item) { IsLocked = true });
            break;
          case NodeTypes.Continuous:
            Items.Add(new IteCONTINUOUS_VM(item) { IsLocked = true });
            break;
          case NodeTypes.ConditionTrue:
            Items.Add(new IteTRUE_VM(item) { IsLocked = true });
            break;
          case NodeTypes.ConditionFalse:
            Items.Add(new IteFALSE_VM(item) { IsLocked = true });
            break;
          case NodeTypes.ConditionTrue1:
            Items.Add(new IteTRUE1_VM(item) { IsLocked = true });
            break;
          case NodeTypes.ConditionFalse1:
            Items.Add(new IteFALSE1_VM(item) { IsLocked = true });
            break;
          case NodeTypes.SetVariable:
            Items.Add(new IteSETVAR_VM(item));
            break;
          case NodeTypes.SetMode:
            Items.Add(new IteSETMODE_VM(item));
            break;
          case NodeTypes.Return:
            Items.Add(new IteRETURN_VM(item));
            break;
          case NodeTypes.Alarm:
            Items.Add(new IteALARM_VM(item));
            break;
          default:
            break;
        }
      }
    }
    public string Name
    {
      get { return Node.Name; }
      set
      {
        if (value != Node.Name)
        {
          Node.Name = value;
          this.OnPropertyChanged("Name");
        }
      }
    }
    public bool IsLocked { get; set; }
    public string Description
    {
      get { return Node.Description(); }
    }
    public NodeTypes NodeType
    {
      get { return Node.NodeType; }
    }
    public List<NodeTypes> AllowedNodes
    {
      get { return Node.AllowedNodes; }
    }
    protected virtual void OnPropertyChanged(string propertyName)
    {
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public bool IsSelected
    {
      get { return _isSelected; }
      set
      {
        if (value != _isSelected)
        {
          _isSelected = value;
          this.OnPropertyChanged("IsSelected");
        }
      }
    }
    public bool IsExpanded
    {
      get { return _isExpanded; }
      set
      {
        if (value != _isExpanded)
        {
          _isExpanded = value;
          this.OnPropertyChanged("IsExpanded");
        }
        // Expand all the way up to the root.
        if (_isExpanded && Parent != null) { Parent.IsExpanded = true; }
      }
    }
    private System.Windows.Media.Brush _background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Transparent);
    public System.Windows.Media.Brush Background
    {
      get
      {
        return _background;
      }
      set
      {
        _background = value;
        this.OnPropertyChanged("Background");
      }
    }
  }
  public class IteCONTINUOUS_VM : IteNodeViewModel
  {
    public IteCONTINUOUS_VM(INode node) : base(node) { }
  }
  public class IteMODE_VM : IteNodeViewModel
  {
    public IteMODE_VM(INode node) : base(node) { }
  }
  public class IteCONDITION_VM : IteNodeViewModel
  {
    public IteCONDITION_VM(INode node) : base(node) { }
  }
  public class IteTRUE_VM : IteNodeViewModel
  {
    public IteTRUE_VM(INode node) : base(node) { }
  }
  public class IteTRUE1_VM : IteNodeViewModel
  {
    public IteTRUE1_VM(INode node) : base(node) { }
  }
  public class IteFALSE_VM : IteNodeViewModel
  {
    public IteFALSE_VM(INode node) : base(node) { }
  }
  public class IteFALSE1_VM : IteNodeViewModel
  {
    public IteFALSE1_VM(INode node) : base(node) { }
  }
  public class IteFIRST_VM : IteNodeViewModel
  {
    public IteFIRST_VM(INode node) : base(node) { }
  }
  public class IteTIMER_VM : IteNodeViewModel
  {
    public IteTIMER_VM(INode node) : base(node) { }
  }
  public class IteSETVAR_VM : IteNodeViewModel
  {
    public IteSETVAR_VM(INode node) : base(node) { }
  }
  public class IteSETMODE_VM : IteNodeViewModel
  {
    public IteSETMODE_VM(INode node) : base(node) { }
  }
  public class IteRETURN_VM : IteNodeViewModel
  {
    public IteRETURN_VM(INode node) : base(node) { }
  }
  public class IteALARM_VM : IteNodeViewModel
  {
    public IteALARM_VM(INode node) : base(node) { }
  }
  public class IteALARMCLOSE_VM : IteNodeViewModel
  {
    public IteALARMCLOSE_VM(INode node) : base(node) { }
  }
  public class Variable : INotifyPropertyChanged
  {
    private VariableType _varType;
    public VariableType VarType
    {
      get { return _varType; }
      private set
      {
        if (_varType.Value != value.Value)
        {
          _varType = value;
          NotifyPropertyChanged("VarType");
        }
      }
    }
    private IoControllers _ioController;
    public IoControllers IoController
    {
      get { return _ioController; }
      set
      {
        if (_ioController != value)
        {
          _ioController = value;
          NotifyPropertyChanged("VarType");
        }
      }
    }
    private dynamic _value;
    public dynamic Value
    {
      get { return _value; }
      set
      {
        if (!value.Equals(_value))
          //if (value != _value)
        {
          var t = value.GetType();
          if (t == typeof(bool))
          {
            VarType = VariableTypes.boolType;
            _value = (bool)value;
          }
          else if (t == typeof(decimal) || t == typeof(double) || t == typeof(int) || t == typeof(long) || t == typeof(short)|| t == typeof(byte))
          {
            t = (_value??(decimal)0).GetType();
            if ((t != typeof(decimal) && t != typeof(double) && t != typeof(int) && t != typeof(long) && t != typeof(short) && t != typeof(byte)) || value != _value)
            {
              VarType = VariableTypes.numberType;
              _value = value;

            }
          }
          if (t == typeof(string))
          {
            VarType = VariableTypes.stringType;
            _value = (string)value;
          }
          NotifyPropertyChanged("Value");
        }
      }
    }
    public dynamic UsersLastValue { get; set; }
    private string _name;
    public string Name
    {
      get { return _name; }
      set
      {
        if (value != _name)
        {
          _name = value;
          NotifyPropertyChanged("Name");
        }
      }
    }
    private string _note;
    public string Note
    {
      get { return _note; }
      set
      {
        if (value != _note)
        {
          _note = value;
          NotifyPropertyChanged("Note");
        }
      }
    }
    private decimal _channel;
    public decimal Channel
    {
      get { return _channel; }
      set
      {
        if (value != _channel)
        {
          _channel = value;
          try
          {
            ChannelWord = (short)Math.Truncate(value);
            ChannelBit = (short)Math.Truncate(( value - ChannelWord)*100);
          }
          catch (Exception)
          {
            ChannelWord = 0;
            ChannelBit = 0;
          }
          NotifyPropertyChanged("Channel");
        }
      }
    }
    public short ChannelWord { get; set; }
    public short ChannelBit { get; set; }
    private bool? _isOutput;
    public bool? IsOutput
    {
      get { return _isOutput; }
      set
      {
        if (value != _isOutput)
        {
          _isOutput = value;
          NotifyPropertyChanged("IsOutput");
        }
      }
    }
    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged(String info)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
    }
  }
  [TypeConverter(typeof(EnumDescriptionTypeConverter))]
  public enum AssignMethod
  {
    [Description("assign ( = )")]
    assign = 1,
    [Description("increment ( + = )")]
    increment = 2,
    [Description("decrement ( - = )")]
    decrement = 3,
    [Description("invert ( ! = )")]
    invert = 4
  }
  public enum VarType
  {
    boolType = 1,
    stringType = 2,
    numberType = 3
  }
  [TypeConverter(typeof(EnumDescriptionTypeConverter))]
  public enum SupportedAdvantechUnits
  {
    [Description("Adam 6000")]
    Adam6000 = Advantech.Adam.AdamType.Adam6000,
  }
  [TypeConverter(typeof(EnumDescriptionTypeConverter))]
  public enum ProtocolTypes
  {
    [Description("TCP")]
    Tcp = System.Net.Sockets.ProtocolType.Tcp,
    [Description("UDP")]
    Udp = System.Net.Sockets.ProtocolType.Udp,
    [Description("IP")]
    IP = System.Net.Sockets.ProtocolType.IP,
    [Description("IPv4")]
    IPv4 = System.Net.Sockets.ProtocolType.IPv4,
    [Description("IPv6")]
    IPv6 = System.Net.Sockets.ProtocolType.IPv6,
    [Description("Unknown")]
    Unknown = System.Net.Sockets.ProtocolType.Unknown,
    [Description("Unspecified")]
    Unspecified = System.Net.Sockets.ProtocolType.Unspecified,
  }
  public struct VariableType
  {
    private readonly string _friendlyName;
    public readonly int Value;
    public readonly VarType enumValue;
    public override string ToString() { return _friendlyName; }
    public VariableType(VarType type)
    {
      Value = (int)type;
      switch (type)
      {
        default:
          _friendlyName = "bit";
          enumValue = VarType.boolType;
          break;
        case VarType.stringType:
          _friendlyName = "text";
          enumValue = VarType.stringType;
          break;
        case VarType.numberType:
          _friendlyName = "number";
          enumValue = VarType.numberType;
          break;
      }
    }
  }
  public static class VariableTypes
  {
    public static VariableType boolType = new VariableType(VarType.boolType);
    public static VariableType stringType = new VariableType(VarType.stringType);
    public static VariableType numberType = new VariableType(VarType.numberType);
  }
}
