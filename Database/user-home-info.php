<?php
require_once './core/Database.php';

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

function updateLastLogin($pdo, $user_id, $loginDate)
{
    $sql = "UPDATE users_info SET last_login = :last_login WHERE user_id = :user_id;";
    try {
        $smtp = $pdo->prepare($sql);
        $smtp->bindParam(':user_id', $user_id);
        $smtp->bindParam(':last_login', $loginDate);
        return $smtp->execute();
    } catch (PDOException $e) {
        echo $e->getMessage();
        return false;
    }
}

if ($_SERVER['REQUEST_METHOD'] == 'POST') {
    $pdo = Database::getInstance()->getPDO();

    $body = file_get_contents('php://input');
    $data = json_decode($body, true);

    if (!$data || !isset($data['user_id'], $data['password'])) {
        echo json_encode(['status' => 'error', 'message' => 'Invalid JSON']);
        exit;
    }

    $user_id = $data['user_id'];
    $password = $data['password'];

    date_default_timezone_set('Asia/Tokyo');
    $loginDate = date("Y-m-d H:i:s");
    $user = selectUserData($pdo, $user_id);

    if (!$user) {
        echo json_encode([
            'status'  => 'error',
            'message' => '入力されたIDが見つかりませんでした。もう一度やり直してください。',
            'user_id' => null,
        ], JSON_UNESCAPED_UNICODE);
    } else if ($user && !password_verify($password, $user['password'])) {
        echo json_encode([
            'status'  => 'error',
            'message' => 'パスワードが違います。もう一度やり直してください。',
            'user_id' => null,
        ], JSON_UNESCAPED_UNICODE);
    } else {
        updateLastLogin($pdo, $user_id, $loginDate);

        echo json_encode([
            'status'  => 'success',
            'message' => 'User created successfully',
            'user_id' => $user_id,
        ], JSON_UNESCAPED_UNICODE);
    }
}
