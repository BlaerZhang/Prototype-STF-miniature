using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using JoostenProductions;

public class ResetLevel : OverridableMonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public override void UpdateMe()
    {
        		if(Keyboard.current.rKey.isPressed){
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); 
		}
    }
}
