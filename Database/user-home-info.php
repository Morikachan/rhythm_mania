<?php
require_once './core/Database.php';

if ($_SERVER["REQUEST_METHOD"] !== "POST") {
    http_response_code(405);
    exit;
}

function selectUserData($pdo, $user_id)
{
    $sql = "SELECT * FROM users_info WHERE user_id = :user_id";
    try {
        $stmt = $pdo->prepare($sql);
        $stmt->bindParam(':user_id', $user_id);
        $stmt->execute();
        $user = $stmt->fetch(PDO::FETCH_ASSOC);
        return $user;
    } catch (PDOException $e) {
        // echo $e->getMessage();
        return false;
    }
}

function selectUserHomeCharacter($pdo, $user_id)
{
    $sql = "SELECT home_card_id FROM user_home WHERE user_id = :user_id";
    try {
        $stmt = $pdo->prepare($sql);
        $stmt->bindParam(':user_id', $user_id);
        $stmt->execute();
        $character = $stmt->fetch(PDO::FETCH_ASSOC);
        return $character;
    } catch (PDOException $e) {
        // echo $e->getMessage();
        return false;
    }
}

function selectLevelRequirements($pdo)
{
    $sql = "SELECT * FROM m_user_lvl";
    try {
        $stmt = $pdo->prepare($sql);
        $stmt->execute();
        $levelTable = $stmt->fetchAll(PDO::FETCH_ASSOC);
        return $levelTable;
    } catch (PDOException $e) {
        // echo $e->getMessage();
        return false;
    }
}

function selectUserItemInventory($pdo, $user_id)
{
    $sql = "SELECT item_id, amount FROM users_inventory WHERE user_id = :user_id";
    try {
        $stmt = $pdo->prepare($sql);
        $stmt->bindParam(':user_id', $user_id);
        $stmt->execute();
        $user_items = $stmt->fetchAll(PDO::FETCH_ASSOC);
        return $user_items;
    } catch (PDOException $e) {
        // echo $e->getMessage();
        return false;
    }
}

    $pdo = Database::getInstance()->getPDO();

    $body = file_get_contents('php://input');
    $data = json_decode($body, true);

    if (!$data || !isset($data['user_id'])) {
        echo json_encode(['status' => 'error', 'message' => 'Invalid JSON']);
        exit;
    }

    $user_id = $data['user_id'];
    
    date_default_timezone_set('Asia/Tokyo');

    $user = selectUserData($pdo, $user_id);

    $home_character_id = selectUserHomeCharacter($pdo, $user_id)['home_card_id'];
    $levels_table = selectLevelRequirements($pdo);

    $user_exp = $user['user_exp'];
    $user_level = 1;
    $next_level_exp = null;

    foreach ($levels_table as $key => $level) {
        if ($user_exp < $level["exp_amount"]) {
            $next_level_exp = $level["exp_amount"];
            break;
        }
        $user_level = $level["lvl"];
    }

    $user_items = selectUserItemInventory($pdo, $user_id);

    if (!$user) {
        echo json_encode([
            'status'  => 'error',
            'message' => '入力されたIDが見つかりませんでした。もう一度やり直してください。',
            'user_info' => [
                'user_name' => '',
                'user_lvl' => 0,
                'user_exp' => 0,
                'next_lvl_value' => 0,
                'next_lvl_percent' => 0,
                'home_card_id' => 0
            ],
            'user_inventory' => [
                'free_gems' => 0,
                'paid_gems' => 0,
                'coins' => 0,
            ],
        ], JSON_UNESCAPED_UNICODE);
    } else {
        echo json_encode([
            'status'  => 'success',
            'message' => 'User info get successfully',
            'user_info' => [
                'user_name' => $user['username'],
                'user_lvl' => $user_level,
                'user_exp' => $user_exp,
                'next_lvl_value' => $next_level_exp,
                'next_lvl_percent' => floor($user_exp * 100 / $next_level_exp),
                'home_card_id' => $home_character_id,
            ],
            'user_inventory' => [
                'free_gems' => $user_items[array_search(1, array_column($user_items, 'item_id'))]['amount'],
                'paid_gems' => $user_items[array_search(2, array_column($user_items, 'item_id'))]['amount'],
                'coins' => $user_items[array_search(3, array_column($user_items, 'item_id'))]['amount'],
            ],
        ], JSON_UNESCAPED_UNICODE);
    }