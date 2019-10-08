using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Rox
{
  public static class Logic
  {
    public new static bool Equals(dynamic a, dynamic b)
    {
      if (a.GetType() != b.GetType()) { throw new Exception("Type Mismatch"); }
      return a.Equals(b);
    }
    public static bool GreaterThan(dynamic a, dynamic b)
    {
      return b > a;
    }
    public static bool LessThan(dynamic a, dynamic b)
    {
      return b < a;
    }
    public static bool GreaterThanOrEqual(dynamic a, dynamic b)
    {
      return b >= a;
    }
    public static bool LessThanOrEqual(dynamic a, dynamic b)
    {
      return b <= a;
    }
    public static bool NotEqual(dynamic a, dynamic b)
    {
      return b != a;
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
  }
  public class IteMode : INode
  {
    public NodeTypes NodeType { get; } = NodeTypes.Mode;
    public string Name { get; set; }
    public List<NodeTypes> AllowedNodes { get; } = new List<NodeTypes>() { };
    public Collection<INode> Items { get; set; } = new Collection<INode>();
    public string Description() { return "{ MODE } A mode can be created to easily abort a running mode and start new sequencing. Stop and Auto modes will run with Start/Stop button. No items can be added directly. Add items to Initialize or Continuous branches."; }
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
    public string Description() { return "{ Timer } TODO: Populate this content."; }
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
            Items.Add(new IteNodeViewModel(item));
            break;
          case NodeTypes.Timer:
            Items.Add(new IteTIMER_VM(item));
            break;
          case NodeTypes.Initialized:
            Items.Add(new IteFIRST_VM(item));
            break;
          case NodeTypes.Continuous:
            Items.Add(new IteCONTINUOUS_VM(item));
            break;
          case NodeTypes.ConditionTrue:
            Items.Add(new IteTRUE_VM(item));
            break;
          case NodeTypes.ConditionFalse:
            Items.Add(new IteFALSE_VM(item));
            break;
          default:
            break;
        }
      }
      //Items = new Collection<IteNodeViewModel>((from item in node.Items select new IteNodeViewModel(item, this)).ToList<IteNodeViewModel>());
      //_clickCommand = new DelegateCommand<string>(
      //           (s) =>
      //           {
      //               Console.WriteLine(node.Description());
      //           }, //Execute
      //           (s) => { return true; } //CanExecute
      //           );
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
        //if (!(_background as System.Windows.Media.SolidColorBrush).Equals(value))
        //{
        //if (_background.ToString()!=value.ToString())
        //{
        _background = value;
        this.OnPropertyChanged("Background");
        //}
        //}
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
  public class IteFALSE_VM : IteNodeViewModel
  {
    public IteFALSE_VM(INode node) : base(node) { }
  }
  public class IteFIRST_VM : IteNodeViewModel
  {
    public IteFIRST_VM(INode node) : base(node) { }
  }
  public class IteTIMER_VM : IteNodeViewModel
  {
    public IteTIMER_VM(INode node) : base(node) { }
  }
  public class Variable : INotifyPropertyChanged
  {
    public VariableType VarType { get; private set; }
    private dynamic _value;
    public dynamic Value
    {
      get { return _value; }
      set
      {
        var t = value.GetType();
        if (t == typeof(bool))
        {
          VarType = VariableTypes.boolType;
        }
        else if (t == typeof(decimal) || t == typeof(int) || t == typeof(long) || t == typeof(short))
        {
          VarType = VariableTypes.numberType;
        }
        if (t == typeof(string))
        {
          VarType = VariableTypes.stringType;
        }
        if (value != _value)
        {
          _value = value;
          NotifyPropertyChanged("Value");
        }
      }
    }
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
    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged(String info)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
    }
  }
  public enum VarType
  {
    boolType,
    stringType,
    numberType
  }
  public struct VariableType
  {
    private readonly string _friendlyName;
    public readonly int Value;
    public override string ToString() { return _friendlyName; }
    public VariableType(VarType type)
    {
      Value = (int)type;
      switch (type)
      {
        default:
          _friendlyName = "bit";
          break;
        case VarType.stringType:
          _friendlyName = "text";
          break;
        case VarType.numberType:
          _friendlyName = "number";
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
