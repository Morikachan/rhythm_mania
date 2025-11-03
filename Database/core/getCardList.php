<?php

require_once './Database.php';

session_start();

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

if ($_SERVER['REQUEST_METHOD'] == 'POST') {
    $pdo = Database::getInstance()->getPDO();
    $user_id = $_SESSION['user']['user_id'];
    $cardList = selectUserCards($pdo, $user_id);
    echo $cardList ? json_encode(['status' => true, 'cardList' => $cardList]) : json_encode(['status' => false]);
    exit();
}
