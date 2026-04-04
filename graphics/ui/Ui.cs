using Godot;
using Godot.Collections;
using System;
using System.Linq;

public partial class Ui : Control
{
	BoxContainer heartContainer;
	PackedScene heartScene = (PackedScene)ResourceLoader.Load("res://graphics/ui/heart.tscn");
	public Control spellControl;
	TextureRect spellTexture;
	Texture2D fireTexture = (Texture2D)ResourceLoader.Load("res://graphics/ui/fire.png");
	Texture2D healTexture = (Texture2D)ResourceLoader.Load("res://graphics/ui/heal.png");
	TextureProgressBar energyBar;
	public TextureProgressBar staminaBar;

	public Dictionary<int, string> Spells = new Dictionary<int, string>
	{
		[0] = "Fireball",
		[1] = "Heal"
	};

	public int currentSpell = 0;

	public override void _Ready()
	{
		heartContainer = this.GetNode<BoxContainer>("Hearts/MarginContainer/HBoxContainer");
		spellControl = this.GetNode<Control>("Spells");
		spellTexture = this.GetNode<TextureRect>("Spells/MarginContainer/TextureRect");
		energyBar = this.GetNode<TextureProgressBar>("EnergyBar/MarginContainer/TextureProgressBar");
		staminaBar = this.GetNode<TextureProgressBar>("StaminaBar/CenterContainer/MarginContainer/TextureProgressBar");
	}

	public async void Setup(int value)
	{
		for (int hearts = 0; hearts < value; hearts++)
		{
			Heart heart = (Heart)heartScene.Instantiate();
			heartContainer.AddChild(heart);
			heart.ChangeAlpha(1.0f);
			SceneTreeTimer timer = this.GetTree().CreateTimer(0.3);
			await ToSignal(timer, "timeout");
		}
	}

	public void UpdateHealth(int value, int direction)
	{
		foreach (Heart child in heartContainer.GetChildren())
		{
			child.QueueFree();
		}

		if (direction < 0)
		{
			for (int hearts = 0; hearts < value; hearts++)
			{
				Heart heart = (Heart)heartScene.Instantiate();
				heartContainer.AddChild(heart);
			}
			Heart extraHeart = (Heart)heartScene.Instantiate();
			heartContainer.AddChild(extraHeart);
			extraHeart.ChangeAlpha(0);
		}
		else
		{
			for (int hearts = 0; hearts < value - 1; hearts++)
			{
				Heart heart = (Heart)heartScene.Instantiate();
				heartContainer.AddChild(heart);
			}
			Heart extraHeart = (Heart)heartScene.Instantiate();
			heartContainer.AddChild(extraHeart);
			extraHeart.ChangeAlpha(1.0f);
		}
	}

	public void UpdateSpell(Dictionary<int, string> spells, int currentSpell)
	{
		if (currentSpell == GetSpellKey("Fireball"))
		{
			spellTexture.Texture = fireTexture;
		}
		else if (currentSpell == GetSpellKey("Heal"))
		{
			spellTexture.Texture = healTexture;
		}
	}

	public int GetSpellKey(string value)
	{
		return Spells.FirstOrDefault(s => s.Value == value).Key;
	}

	public void UpdateEnergy(int value)
	{
		energyBar.Value = value;
	}

	public void UpdateStamina(int current, int target)
	{
		Tween tween = this.CreateTween();
		tween.TweenMethod(new Callable(this, nameof(_ChangeStamina)), current, target, 0.25f);
	}

	private void _ChangeStamina(int value)
	{
		staminaBar.Value = value;
	}

	public void ChangeStaminaAlpha(float value)
	{
		Tween tween = this.CreateTween();
		tween.TweenMethod(new Callable(this, nameof(_ChangeStaminaAlpha)), 1.0 - value, value, 0.25);
	}

	private void _ChangeStaminaAlpha(float value)
	{
		Color modulate = staminaBar.Modulate;
		modulate.A = value;
		staminaBar.Modulate = modulate;
	}
}
