<?php
require_once './core/Database.php';

if ($_SERVER["REQUEST_METHOD"] !== "POST") {
    http_response_code(405);
    exit;
}

function searchID($pdo, $id)
{
    $sql = "SELECT * FROM users_info WHERE user_id=:id";
    $stmt = $pdo->prepare($sql);
    $stmt->bindParam(':id', $id);
    $stmt->execute();
    return $stmt->fetch(PDO::FETCH_ASSOC);
}

function generateSecureRandomString($length = 6)
{
    $characters = '0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ';
    $randomString = '';
    for ($i = 0; $i < $length; $i++) {
        $randomString .= $characters[random_int(0, strlen($characters) - 1)];
    }
    return $randomString;
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

function createUser($pdo, $id, $password, $email, $username, $registrationDate)
{
    $sql = "INSERT INTO users_info (user_id, password, email, username, registration_date, user_exp) 
    VALUES (:user_id, :password, :email, :username, :registration_date, :user_exp)";
    try {
        $passwordHash = password_hash($password, PASSWORD_DEFAULT);
        $userExpDefault = 0;

        $smtp = $pdo->prepare($sql);
        $smtp->bindParam(':user_id', $id);
        $smtp->bindParam(':password', $passwordHash);
        $smtp->bindParam(':email', $email);
        $smtp->bindParam(':username', $username);
        $smtp->bindParam(':registration_date', $registrationDate);
        $smtp->bindParam(':user_exp', $userExpDefault);
        return $smtp->execute();
    } catch (PDOException $e) {
        echo $e->getMessage();
        return false;
    }
}

function addInventoryCard($pdo, $user_id, $card_id, $add_date)
{
    $sql = "INSERT INTO card_inventory (user_id, card_id, add_date, card_exp) VALUES (:user_id, :card_id, :add_date, :card_exp)";
    try {
        $cardExpDefault = 0;

        $smtp = $pdo->prepare($sql);
        $smtp->bindParam(':user_id', $user_id);
        $smtp->bindParam(':card_id', $card_id);
        $smtp->bindParam(':add_date', $add_date);
        $smtp->bindParam(':card_exp', $cardExpDefault);
        return $smtp->execute();
    } catch (PDOException $e) {
        // echo $e->getMessage();
        return false;
    }
}

function addHomeChar($pdo, $user_id, $card_id)
{
    $sql = "INSERT INTO user_home (user_id, home_card_id) VALUES (:user_id, :home_card_id)";
    try {
        $smtp = $pdo->prepare($sql);

        $smtp->bindParam(':user_id', $user_id);
        $smtp->bindParam(':home_card_id', $card_id);
        return $smtp->execute();
    } catch (PDOException $e) {
        // echo $e->getMessage();
        return false;
    }
}

function addInventoryItem($pdo, $user_id, $item_id, $amount)
{
    $sql = "INSERT INTO users_inventory (user_id, item_id, amount) VALUES (:user_id, :item_id, :amount)";
    try {
        $smtp = $pdo->prepare($sql);
        $smtp->bindParam(':user_id', $user_id);
        $smtp->bindParam(':item_id', $item_id);
        $smtp->bindParam(':amount', $amount);
        return $smtp->execute();
    } catch (PDOException $e) {
        echo $e->getMessage();
        return false;
    }
}


$pdo = Database::getInstance()->getPDO();
$body = file_get_contents('php://input');
$data = json_decode($body, true);

if (!$data || !isset($data['username'], $data['email'], $data['password'])) {
    echo json_encode(['status' => 'error', 'message' => 'Invalid JSON']);
    exit;
}

$username = $data['username'];
$email = $data['email'];
$password = $data['password'];

date_default_timezone_set('Asia/Tokyo');
$formattedRegistrationDate = date("Y-m-d H:i:s");

$random_id = generateSecureRandomString(6);

while (searchID($pdo, $random_id)) {
    $random_id = generateSecureRandomString(6);
}

$result = createUser($pdo, $random_id, $password, $email, $username, $formattedRegistrationDate);
if ($result) {
    // create user inventory for 4 cards R from id 1 to 4
    // add to home char with 1-st id
    $newUserData = selectUserData($pdo, $random_id);

    for ($i = 1; $i <= 4; $i++) {
        addInventoryCard($pdo, $newUserData['user_id'], $i, $formattedRegistrationDate);
    }

    addHomeChar($pdo, $newUserData['user_id'], 1);
    addInventoryItem($pdo, $newUserData['user_id'], 1, 2500);
    addInventoryItem($pdo, $newUserData['user_id'], 2, 0);
    addInventoryItem($pdo, $newUserData['user_id'], 3, 100000);
    addInventoryItem($pdo, $newUserData['user_id'], 4, 100);

    echo json_encode([
        'status'  => 'success',
        'message' => 'User created successfully',
        'user_id' => $newUserData['user_id'],
    ], JSON_UNESCAPED_UNICODE);
} else {
    echo json_encode([
        'status'  => 'error',
        'message' => 'Failed to create user',
        'user_id' => null,
    ], JSON_UNESCAPED_UNICODE);
}
