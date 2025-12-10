<?php
require_once './core/Database.php';

if ($_SERVER["REQUEST_METHOD"] !== "GET") {
    http_response_code(405);
    exit;
}

function selectSongList($pdo, $user_id)
{
    $sql ="SELECT 
                s.song_id,
                s.song_name,
                s.song_level,
                s.song_bpm,
                COALESCE(usi.best_score, 0) AS best_score,
                COALESCE(usi.best_combo, '—') AS best_combo
            FROM songs AS s
            LEFT JOIN users_song_results AS usi
                ON usi.song_id = s.song_id
                AND usi.user_id = :user_id
            ORDER BY s.song_id;
        ";
    try {
        $stmt = $pdo->prepare($sql);
        $stmt->bindParam(':user_id', $user_id);
        $stmt->execute();
        $songs = $stmt->fetchAll(PDO::FETCH_ASSOC);
        return $songs;
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

    $song_list = selectSongList($pdo, $user_id);

    if (!$song_list) {
        echo json_encode([
            'status'  => 'error',
            'message' => '曲リストの取得中にエラーが発生しました。',
            'song_list' => [
                'song_id' => 0,
                'song_name' => '',
                'song_level' => 0,
                'song_bpm' => 0,
                'best_score' => 0,
                'best_combo' => '—',
            ],
        ], JSON_UNESCAPED_UNICODE);
    } else {
        echo json_encode([
            'status'  => 'success',
            'message' => '曲リストの取得ができました。',
            'song_list' => $song_list,
        ], JSON_UNESCAPED_UNICODE);
    }