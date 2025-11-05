<?php
require_once './core/Database.php';


if ($_SERVER["REQUEST_METHOD"] !== "POST") {
    http_response_code(405);
    exit;
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

$pdo = Database::getInstance()->getPDO();
$body = file_get_contents('php://input');
$data = json_decode($body, true);

if (!$data || !isset($data['user_id'])) {
    echo json_encode(['status' => 'error', 'message' => 'Invalid JSON']);
    exit;
}

$user_id = $data['user_id'];

date_default_timezone_set('Asia/Tokyo');
$loginDate = date("Y-m-d H:i:s");
$user = selectUserData($pdo, $user_id);

if (!$user) {
    echo json_encode([
        'status'  => 'error',
        'message' => '入力されたIDが見つかりませんでした。もう一度やり直してください。',
    ], JSON_UNESCAPED_UNICODE);
} else {
    updateLastLogin($pdo, $user_id, $loginDate);
    echo json_encode([
        'status'  => 'success',
        'message' => 'User login updated successfully',
    ], JSON_UNESCAPED_UNICODE);
}
