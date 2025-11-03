<?php

require_once './Database.php';

session_start();

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

if ($_SERVER['REQUEST_METHOD'] == 'POST') {
    $pdo = Database::getInstance()->getPDO();
    $levelTable = selectLevelRequirements($pdo);
    echo $levelTable ? json_encode(['status' => true, 'levelTable' => $levelTable]) : json_encode(['status' => false]);
    exit();
}
