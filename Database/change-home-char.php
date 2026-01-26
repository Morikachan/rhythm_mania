<?php
require_once './core/Database.php';

if ($_SERVER["REQUEST_METHOD"] !== "POST") {
    http_response_code(405);
    exit;
}

function changeHomeChar($pdo, $user_id, $card_id)
{
    $sql = "UPDATE user_home SET home_card_id = :home_card_id WHERE user_id = :user_id;";
    try {
        $smtp = $pdo->prepare($sql);
        $smtp->bindParam(':user_id', $user_id);
        $smtp->bindParam(':home_card_id', $card_id);
        return $smtp->execute();
    } catch (PDOException $e) {
        echo $e->getMessage();
        return false;
    }
}

$pdo = Database::getInstance()->getPDO();

$body = file_get_contents('php://input');
$data = json_decode($body, true);

if (!$data || !isset($data['user_id'], $data['card_id'])) {
    echo json_encode(['status' => 'error', 'message' => 'Invalid JSON or missing song_id']);
    exit;
}

$user_id = $data['user_id'];
$card_id = $data['card_id'];

$result = changeHomeChar($pdo, $user_id, $card_id);

if (!$result) {
    echo json_encode([
        'status'  => 'error',
        'message' => 'Set home card error.',
    ], JSON_UNESCAPED_UNICODE);
} else {
    echo json_encode([
        'status'  => 'success',
        'message' => 'Set home card success.',
    ], JSON_UNESCAPED_UNICODE);
}
