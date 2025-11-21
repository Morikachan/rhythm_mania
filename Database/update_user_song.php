<?php

require_once './core/Database.php';

if ($_SERVER["REQUEST_METHOD"] !== "POST") {
    http_response_code(405);
    exit;
}

function selectSongList($pdo, $song_id, $user_id)
{
    $sql = "SELECT best_score, best_combo
            FROM users_song_results
            WHERE song_id = :song_id AND user_id = :user_id;";

    try {
        $stmt = $pdo->prepare($sql);
        $stmt->bindParam(':song_id', $song_id);
        $stmt->bindParam(':user_id', $user_id);
        $stmt->execute();
        $songs = $stmt->fetch(PDO::FETCH_ASSOC);
        return $songs;
    } catch (PDOException $e) {
        //echo $e->getMessage();
        return false;
    }
}

function setNewSongInfo($pdo, $song_id, $user_id, $combo, $score)
{
    $sql = "INSERT INTO users_song_results (user_id, song_id, best_combo, best_score)
            VALUES(:user_id, :song_id, :best_combo, :best_score);";

    try {
        $stmt = $pdo->prepare($sql);
        $stmt->bindParam(':user_id', $user_id);
        $stmt->bindParam(':song_id', $song_id);
        $stmt->bindParam(':best_score', $score);
        $stmt->bindParam(':best_combo', $combo);
        return $stmt->execute();
    } catch (PDOException $e) {
        //echo $e->getMessage();
        return false;
    }
}

function updateSongCombo($pdo, $song_id, $user_id, $combo)
{
    $sql = "UPDATE users_song_results SET best_combo = :best_combo
            WHERE song_id = :song_id AND user_id = :user_id;";

    try {
        $stmt = $pdo->prepare($sql);
        $stmt->bindParam(':best_combo', $combo);
        $stmt->bindParam(':song_id', $song_id);
        $stmt->bindParam(':user_id', $user_id);
        return $stmt->execute();
    } catch (PDOException $e) {
        //echo $e->getMessage();
        return false;
    }
}

function updateSongScore($pdo, $song_id, $user_id, $score)
{
    $sql = "UPDATE users_song_results SET best_score = :best_score
            WHERE song_id = :song_id AND user_id = :user_id;";

    try {
        $stmt = $pdo->prepare($sql);
        $stmt->bindParam(':best_score', $score);
        $stmt->bindParam(':song_id', $song_id);
        $stmt->bindParam(':user_id', $user_id);
        return $stmt->execute();
    } catch (PDOException $e) {
        //echo $e->getMessage();
        return false;
    }
}

function getComboIndex($combo)
{
    $combo_info = array("D", "C", "B", "A", "S", "SS");
    $index = array_search($combo, $combo_info);
    return $index;
}

$pdo = Database::getInstance()->getPDO();

$body = file_get_contents('php://input');
$data = json_decode($body, true);

if (!$data || !isset($data['user_id']) || !isset($data['score']) || !isset($data['combo']) || !isset($data['song_id'])) {
    echo json_encode(['status' => 'error', 'message' => 'Invalid JSON']);
    exit;
}

$user_id = $data['user_id'];
$song_id = $data['song_id'];
$user_combo = $data['combo'];
$user_score = $data['score'];

$song_results_exist = selectSongList($pdo, $song_id, $user_id);
$combo_isNew = false;
$score_isNew = false;

if(empty($song_results_exist)) {
    // Insert with combo and score
    setNewSongInfo($pdo, $song_id, $user_id, $user_combo, $user_score);
    $combo_isNew = true;
    $score_isNew = true;
} else {
    $user_combo_index = getComboIndex($user_combo);
    $past_index = getComboIndex($song_results_exist["best_combo"]);
    $past_score = $song_results_exist["best_score"];

    if ($user_combo_index > $past_index && $user_score > $past_score) {
        // Set new best combo for song and new best score for song
        updateSongScore($pdo, $song_id, $user_id, $user_score);
        updateSongCombo($pdo, $song_id, $user_id, $user_combo);
        $combo_isNew = true;
        $score_isNew = true;
    } else if ($user_score > $past_score && $user_combo_index <= $past_index) {
        // Set only new best score for song
        updateSongScore($pdo, $song_id, $user_id, $user_score);
        $score_isNew = true;
    } else if ($user_score <= $past_score && $user_combo_index > $past_index) {
        // Set only new best combo for song
        updateSongCombo($pdo, $song_id, $user_id, $user_combo);
        $combo_isNew = true;
    }
}

echo json_encode([
            'status'  => 'success',
            'message' => '結果情報の習得ができました。',
            'combo_isNew' => $combo_isNew,
            'score_isNew' => $score_isNew,
        ], JSON_UNESCAPED_UNICODE);