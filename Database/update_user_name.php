<?php

require_once './core/Database.php';

if ($_SERVER["REQUEST_METHOD"] !== "POST") {
    http_response_code(405);
    exit;
}

function updateUserName($pdo, $user_id, $username)
{
    $sql = "UPDATE users_info SET username = :username
            WHERE user_id = :user_id;";

    try {
        $stmt = $pdo->prepare($sql);
        $stmt->bindParam(':user_id', $user_id);
        $stmt->bindParam(':username', $username);
        return $stmt->execute();
    } catch (PDOException $e) {
        //echo $e->getMessage();
        return false;
    }
}

$pdo = Database::getInstance()->getPDO();

$body = file_get_contents('php://input');
$data = json_decode($body, true);

if (!$data || !isset($data['user_id']) || !isset($data['username'] )) {
    echo json_encode(['status' => 'error', 'message' => 'Invalid JSON']);
    exit;
}

$user_id = $data['user_id'];
$username = $data['username'];

$result = updateUserName($pdo, $user_id, $username);

if(!$result) {
    echo json_encode([
            'status'  => 'error',
            'message' => 'Error during User name update',
        ], JSON_UNESCAPED_UNICODE);
} else {
    echo json_encode([
            'status'  => 'success',
            'message' => 'User name updated successfully',
        ], JSON_UNESCAPED_UNICODE);
}