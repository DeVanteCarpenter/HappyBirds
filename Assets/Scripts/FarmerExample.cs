using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farmer: MonoBehaviour
{
    public Animal animal;
    public Sheep sheep;
    public Cow cow;
    public Duck duck;
    public Bear bear;

    public List<Animal> animals = new List<Animal>();

    public void Start()
    {
        //animal.MakeNoise();
        //sheep.MakeNoise(); // prints "HIOSADF!"
        //cow.MakeNoise(); // prints "HIOSADF!"
        //duck.MakeNoise(); // 
        //sheep.SleepNow();
        cow.Init( "cow", Color.black );
        sheep.Init( "Sheep", Color.white );
        cow.Init( "Cow", Color.black );
        animals.Add( cow );
        animals.Add( sheep );
        animals.Add( animal );
    }

    public void SleepTime()
    {
        for( int i = 0; i < animals.Count; i++ )
        {
            animals[i].MakeNoise();
            //animals[i].SleepNow();
        }
    }
}

public class Animal
{
    protected string name;
    public Color color;

    public virtual void Init(string newName, Color newColor)
    {
        name = newName;
        color = newColor;
    }

    public virtual void MakeNoise()
    {
        Debug.Log( "HIOSADF!" );
    }

    protected void SleepNow()
    {
        Debug.Log( "going to sleep" );
    }
}

public class Sheep: Animal
{

    public override void MakeNoise()
    {
        Debug.Log( name + " just made the noise: Baaaa!" );
        SleepNow();
    }
}

public class Cow: Animal
{
    public override void Init( string newName, Color newColor )
    {
        base.Init( newName, newColor );
        if (name == "Cow")
        {
            color = Color.white;
        }
    }

    public override void MakeNoise()
    {
        Debug.Log( "Mooo!" );
    }
}

public class Bull : Cow
{
    public override void MakeNoise()
    {
        Debug.Log( "Rar!" );
    }
}

public class Calf : Cow
{
    public override void MakeNoise()
    {
        Debug.Log( "moo!" );
    }
}

public class Heifer : Cow
{
    public override void MakeNoise()
    {
        Debug.Log( "MOOOOOO!" );
    }
}

public class Duck: Animal
{

}




public class Bear
{

}