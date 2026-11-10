namespace WeComSever.Utils;

public class CronPreset
{
	public string Name { get; set; }

	public string Expression { get; set; }

	public string Description { get; set; }

	public CronPreset(string name, string expression, string description)
	{
		Name = name;
		Expression = expression;
		Description = description;
	}

	public override string ToString()
	{
		return Name;
	}
}
