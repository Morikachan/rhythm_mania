<?php
require_once './core/Database.php';

if ($_SERVER["REQUEST_METHOD"] !== "POST") {
    http_response_code(405);
    exit;
}

function getSongRanking($pdo, $song_id)
{
    $sql = "SELECT 
                ui.username,
                ui.user_id,
                usr.best_score,
                usr.best_combo,
                uh.home_card_id
            FROM users_song_results AS usr
            INNER JOIN users_info AS ui 
                ON usr.user_id = ui.user_id
            INNER JOIN user_home AS uh 
                ON usr.user_id = uh.user_id
            WHERE usr.song_id = :song_id
            ORDER BY usr.best_score DESC";

    try {
        $stmt = $pdo->prepare($sql);
        $stmt->bindParam(':song_id', $song_id, PDO::PARAM_INT);
        $stmt->execute();
        $ranking = $stmt->fetchAll(PDO::FETCH_ASSOC);
        
        foreach ($ranking as $index => &$row) {
            $row['rank'] = $index + 1; 
        }
        unset($row);

        return $ranking;
    } catch (PDOException $e) {
        return false;
    }
}

    $pdo = Database::getInstance()->getPDO();

    $body = file_get_contents('php://input');
    $data = json_decode($body, true);

    if (!$data || !isset($data['song_id'])) {
        echo json_encode(['status' => 'error', 'message' => 'Invalid JSON or missing song_id']);
        exit;
    }

    $song_id = $data['song_id'];

    $ranking_list = getSongRanking($pdo, $song_id);

    if ($ranking_list === false) {
        echo json_encode([
            'status'  => 'error',
            'message' => 'Ranking obtaining error.',
            'ranking' => []
        ], JSON_UNESCAPED_UNICODE);
    } elseif (empty($ranking_list)) {
        echo json_encode([
            'status'  => 'success',
            'message' => 'No ranking results yet.',
            'ranking' => []
        ], JSON_UNESCAPED_UNICODE);
    } else {
        echo json_encode([
            'status'  => 'success',
            'message' => 'Ranking obtaining success.',
            'ranking' => $ranking_list
        ], JSON_UNESCAPED_UNICODE);
    }