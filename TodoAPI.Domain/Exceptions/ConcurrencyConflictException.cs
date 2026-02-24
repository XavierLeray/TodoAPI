namespace TodoAPI.Domain.Exceptions;

/// <summary>
/// Levée quand une modification concurrente est détectée (lost update).
/// 
/// Scénario :
///   1. User A lit le todo (ConcurrencyStamp = "abc-123")
///   2. User B lit le même todo (ConcurrencyStamp = "abc-123")
///   3. User A sauvegarde → OK, ConcurrencyStamp passe à "def-456"
///   4. User B sauvegarde avec ConcurrencyStamp = "abc-123" → CONFLIT
///      La valeur en base ("def-456") ne correspond plus → ConcurrencyConflictException
///
/// Question d'entretien :
///   "Différence entre verrouillage optimiste et pessimiste ?"
///   → Optimiste : on laisse tout le monde lire/écrire, on détecte le conflit au moment du save.
///     Adapté aux cas où les conflits sont rares (majorité des API CRUD).
///   → Pessimiste : on verrouille la ligne au moment du SELECT (SELECT FOR UPDATE).
///     Adapté aux cas où les conflits sont fréquents (stock marketplace).
/// </summary>
public class ConcurrencyConflictException : Exception
{
    public string EntityName { get; }
    public object EntityId { get; }

    public ConcurrencyConflictException(string entityName, object entityId)
        : base($"Conflit de concurrence sur {entityName} (Id: {entityId}). " +
               $"L'entité a été modifiée par un autre utilisateur.")
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}
