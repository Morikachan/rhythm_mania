<?php
require_once './Database.php';

if ($_SERVER["REQUEST_METHOD"] !== "POST") {
    http_response_code(405);
    exit;
}

function selectUserCards($pdo, $user_id)
{
    // $sql = "SELECT * FROM card_inventory WHERE user_id = :user_id";
    $sql = "SELECT * FROM card_inventory
    -- カード全体の情報
    LEFT JOIN card_info 
        ON card_inventory.card_id = card_info.card_id
    -- スキール
    LEFT JOIN m_card_skill 
        ON m_card_skill.card_skill_id = card_info.card_skill_id 
    -- カード全体の情報
    LEFT JOIN m_card_type 
        ON m_card_type.card_type_id = card_info.card_type_id 
    LEFT JOIN m_rarity 
        ON m_rarity.rarity_id = card_info.rarity_id 
    -- TODO カードレベル
    -- LEFT JOIN m_card_type 
    --     ON m_card_skill.card_skill_id = card_info.card_skill_id 
        WHERE card_inventory.user_id = :user_id
        ORDER BY card_info.card_id;";
    try {
        $stmt = $pdo->prepare($sql);
        $stmt->bindParam(':user_id', $user_id);
        $stmt->execute();
        $cardList = $stmt->fetchAll(PDO::FETCH_ASSOC);
        return $cardList;
    } catch (PDOException $e) {
        echo $e->getMessage();
        return false;
    }
}


$pdo = Database::getInstance()->getPDO();

$body = file_get_contents('php://input');
$data = json_decode($body, true);

if (!$data || !isset($data['user_id'])) {
    echo json_encode(['status' => 'error', 'message' => 'Invalid JSON or missing song_id']);
    exit;
}

$user_id = $data['user_id'];

$card_list = selectUserCards($pdo, $user_id);

if ($card_list === false) {
    echo json_encode([
        'status'  => 'error',
        'message' => 'Getting cards error.',
        'cardlist' => []
    ], JSON_UNESCAPED_UNICODE);
} elseif (empty($card_list)) {
    echo json_encode([
        'status'  => 'success',
        'message' => 'No cards yet.',
        'cardlist' => []
    ], JSON_UNESCAPED_UNICODE);
} else {
    echo json_encode([
        'status'  => 'success',
        'message' => 'Cards obtaining success.',
        'cardlist' => $card_list,
    ], JSON_UNESCAPED_UNICODE);
}
