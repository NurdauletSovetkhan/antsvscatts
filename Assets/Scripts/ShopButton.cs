using UnityEngine;
using UnityEngine.UI;

public class ShopButton : MonoBehaviour {
    public Button shopButton;

    void Start() {
        if (shopButton == null) {
            shopButton = GetComponent<Button>();
        }

        if (shopButton != null) {
            shopButton.onClick.AddListener(OpenShop);
        }
    }

    void OpenShop() {
        if (ShopMenu.Instance != null) {
            ShopMenu.Instance.OpenShop();
        }
    }
}
