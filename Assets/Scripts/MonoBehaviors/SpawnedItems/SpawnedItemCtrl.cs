using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the spawned item's behavior, such as movement,rotation, and events that can change game state
/// </summary>
public class SpawnedItemCtrl : MonoBehaviour
{
    /// <summary>
    /// Spawned Item's type
    /// </summary>
    public SpawnedItem.SpawnType SpawnType;

    /// <summary>
    /// Transform where random meshes can be attached to
    /// </summary>
    public Transform meshAnchor;

    /// <summary>
    /// A position determined at its spawn time. Used to calculate the spawned item's direction of movement
    /// </summary>
    private Vector3 m_TargetPos;

    /// <summary>
    /// spawned item's movement speed
    /// </summary>
    private float m_MoveSpeed = 1;

    /// <summary>
    /// spawned item's movement direction
    /// </summary>
    private Vector3 direction;

    /// <summary>
    /// spawned item's overall lifetime. It will autodestroy if not interacted with during it lifetime duration
    /// </summary>
    private const float lifetime = 15;

    /// <summary>
    /// True if movement is forcefully halted
    /// </summary>
    private bool isMovementHalted = false;

    /// <summary>
    /// Rotation controls for the spawned item's mesh
    /// </summary>
    private Vector3 rotationalAxis = Vector3.up;
    private float rotateSpeed = 2;

    /// <summary>
    /// Unity Start method
    /// </summary>
    private void Start()
    {
        m_TargetPos = GameManager.Instance.playerTransform.position;
        //Determine the spawned item's model
        GameObject[] modelCatalog = GameManager.Instance.spawnedItemModelCatalog[SpawnType];
        int modelNdx = Random.Range(0, modelCatalog.Length);
        GameObject instance = Instantiate(modelCatalog[modelNdx], meshAnchor) as GameObject;
        direction = (m_TargetPos - transform.position).normalized;
        if (GameManager.Instance.level >= GameManager.MaxSpeedLevel) m_MoveSpeed = GameManager.MaxItemMoveSpeed;
        else m_MoveSpeed = ((float)GameManager.Instance.level).Remap(1, GameManager.MaxSpeedLevel, GameManager.MinItemMoveSpeed, GameManager.MaxItemMoveSpeed);

        rotationalAxis = new Vector3(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
        StartCoroutine(Autodestruct());
    }

    /// <summary>
    /// Unity Update method
    /// </summary>
    private void Update()
    {
        if (isMovementHalted) return;
        //Gravitate towards target position.
        //transform.position = Vector3.MoveTowards(transform.position, m_TargetPos, m_MoveSpeed * Time.deltaTime);
        transform.Translate(m_MoveSpeed * direction * Time.deltaTime);
        meshAnchor.Rotate(rotationalAxis.normalized * rotateSpeed, Space.Self);
    }

    /// <summary>
    /// Unity OnTriggerEnter
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(Tags.Player))
        {
            LoseGame();
        }
    }

    /// <summary>
    /// Despawn as a result of a successful interaction from the player
    /// </summary>
    public void Despawn()
    {
        GameManager.Instance.AddScore(SpawnType);
        Destroy(gameObject);
    }

    /// <summary>
    /// Forces game to transition into Results game state.
    /// </summary>
    public void LoseGame()
    {
        if (GameManager.Instance.CurrentState != GameManager.AppState.Results) GameManager.Instance.LoseGame();
    }

    /// <summary>
    /// Coroutine to control the lifetime of the spawned item. If spawned item(not an inert waste) is auto-destructed, player also loses the game. 
    /// </summary>
    /// <returns></returns>
    private IEnumerator Autodestruct()
    {
        yield return new WaitForSeconds(lifetime);
        if (SpawnType == SpawnedItem.SpawnType.InertWaste) yield break;
        LoseGame();
    }

    /// <summary>
    /// Called by UI components to control the item's movement behavior
    /// </summary>
    /// <param name="move"></param>
    public void SetMoving(bool move)
    {
        isMovementHalted = !move;
    }

}
