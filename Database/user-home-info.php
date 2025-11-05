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
        echo $e->getMessage();
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
        echo $e->getMessage();
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
        $userItems = $stmt->fetchAll(PDO::FETCH_ASSOC);
        return $userItems;
    } catch (PDOException $e) {
        echo $e->getMessage();
        return false;
    }
}

function selectUsername($pdo, $user_id)
{
    $sql = "SELECT username FROM users_info WHERE user_id = :user_id";
    try {
        $stmt = $pdo->prepare($sql);
        $stmt->bindParam(':user_id', $user_id);
        $stmt->execute();
        $username = $stmt->fetch(PDO::FETCH_ASSOC);
        return $username;
    } catch (PDOException $e) {
        echo $e->getMessage();
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

    $homeCharacter_id = selectUserHomeCharacter($pdo, $user_id)['home_card_id'];
    $levelsTable = selectLevelRequirements($pdo);

    $userExp = $_SESSION['user']['user_exp'];
    $userLevel = 1;
    $nextLevelExp = null;

    $username = selectUsername($pdo, $user_id);
    $_SESSION['user']['username'] = $username['username'];

    foreach ($levelsTable as $key => $level) {
        if ($userExp < $level["exp_amount"]) {
            $nextLevelExp = $level["exp_amount"];
            break;
        }
        $userLevel = $level["lvl"];
    }

    $userItems = selectUserItemInventory($pdo, $_SESSION['user']['user_id']);

    $_SESSION['homeCharacter'] = $homeCharacter_id;
    $_SESSION['userLvl'] = $userLevel;
    $_SESSION['nextLvlValue'] = $nextLevelExp;
    $_SESSION['nextLvlValuePercent'] = floor($userExp * 100 / $nextLevelExp);

    if (!$user) {
        echo json_encode([
            'status'  => 'error',
            'message' => '入力されたIDが見つかりませんでした。もう一度やり直してください。',
            'home_card_id' => null,
            'user_info' => [
                'user_name' => '',
                'user_lvl' => 0,
                'user_exp' => 0,
                'home_card_id' => 0
            ],
            'user_inventory' => [
                'free_gems' => 0,
                'paid_gems' => 0,
                'coins' => 0,
            ],
        ], JSON_UNESCAPED_UNICODE);
    } else {
        updateLastLogin($pdo, $user_id, $loginDate);

        echo json_encode([
            'status'  => 'success',
            'message' => 'User created successfully',
            'home_card_id' => $user_id,
            'user_info' => [
                'user_name' => '',
                'user_lvl' => 0,
                'user_exp' => 0,
                'home_card_id' => 0
            ],
            'user_inventory' => [
                'free_gems' => $userItems[array_search(1, array_column($userItems, 'item_id'))]['amount'],
                'paid_gems' => $userItems[array_search(2, array_column($userItems, 'item_id'))]['amount'],
                'coins' => $userItems[array_search(3, array_column($userItems, 'item_id'))]['amount'],
            ],
        ], JSON_UNESCAPED_UNICODE);
    }