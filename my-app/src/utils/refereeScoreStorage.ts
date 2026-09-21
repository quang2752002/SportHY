import { TranDau, MatchScoreDetails } from '../types/tranDau';

const STORAGE_PREFIX = 'sport_match_score_';

export const refereeScoreStorage = {
  getScoreDetails: (tran: TranDau | null | undefined): MatchScoreDetails => {
    if (!tran || !tran.id) {
      return {
        score1: 0,
        score2: 0,
        setScores: [{ setNumber: 1, score1: 0, score2: 0 }],
        events: [],
      };
    }

    // 1. Kiểm tra localStorage
    if (typeof window !== 'undefined') {
      try {
        const stored = localStorage.getItem(`${STORAGE_PREFIX}${tran.id}`);
        if (stored) {
          const parsed = JSON.parse(stored);
          if (parsed && typeof parsed.score1 === 'number') {
            return parsed;
          }
        }
      } catch (err) {
        console.error('Error reading match score from storage', err);
      }
    }

    // 2. Phân tích từ tran.ghiChu nếu có format lưu trước đó
    if (tran.ghiChu) {
      // Thử parse JSON
      try {
        const trimmed = tran.ghiChu.trim();
        if (trimmed.startsWith('{') && trimmed.endsWith('}')) {
          const parsed = JSON.parse(trimmed);
          if (parsed && typeof parsed.score1 === 'number') {
            return {
              score1: parsed.score1 || 0,
              score2: parsed.score2 || 0,
              winner: parsed.winner,
              setScores: parsed.setScores || [{ setNumber: 1, score1: parsed.score1 || 0, score2: parsed.score2 || 0 }],
              events: parsed.events || [],
              notes: parsed.notes || '',
              actualStartTime: parsed.actualStartTime,
              actualEndTime: parsed.actualEndTime,
              durationMinutes: parsed.durationMinutes,
              extraTimeMinutes: parsed.extraTimeMinutes,
              weatherCondition: parsed.weatherCondition,
              pitchCondition: parsed.pitchCondition,
              spectatorCount: parsed.spectatorCount,
              mvpAthlete: parsed.mvpAthlete,
              winMethod: parsed.winMethod,
              refereeNotes: parsed.refereeNotes,
              supervisorNotes: parsed.supervisorNotes,
              isFinalized: parsed.isFinalized,
              finalizedAt: parsed.finalizedAt,
              finalizedBy: parsed.finalizedBy,
              signedReferee: parsed.signedReferee,
              signedSecretary: parsed.signedSecretary,
              signedTeam1: parsed.signedTeam1,
              signedTeam2: parsed.signedTeam2,
            };
          }
        }
      } catch {}

      // Thử regex: "Tỷ số: X - Y"
      const match = tran.ghiChu.match(/(?:Tỷ số|Tỉ số|Score):\s*(\d+)\s*[-:]\s*(\d+)/i);
      if (match) {
        const s1 = parseInt(match[1], 10);
        const s2 = parseInt(match[2], 10);
        return {
          score1: s1,
          score2: s2,
          winner: s1 > s2 ? 1 : s2 > s1 ? 2 : 'draw',
          setScores: [{ setNumber: 1, score1: s1, score2: s2 }],
          events: [],
          notes: tran.ghiChu,
        };
      }
    }

    return {
      score1: 0,
      score2: 0,
      setScores: [{ setNumber: 1, score1: 0, score2: 0 }],
      events: [],
      notes: tran.ghiChu || '',
    };
  },

  saveScoreDetails: (tranId: number, details: MatchScoreDetails): void => {
    if (typeof window !== 'undefined' && tranId) {
      try {
        localStorage.setItem(`${STORAGE_PREFIX}${tranId}`, JSON.stringify(details));
      } catch (err) {
        console.error('Error saving match score to storage', err);
      }
    }
  },

  /** Serialize thành chuỗi JSON để lưu bền vững vào database qua field tran.ghiChu */
  serializeDetails: (details: MatchScoreDetails): string => {
    try {
      return JSON.stringify(details);
    } catch {
      return '';
    }
  },

  formatScoreSummary: (details: MatchScoreDetails): string => {
    let text = `Tỷ số: ${details.score1} - ${details.score2}`;
    if (details.setScores && details.setScores.length > 1) {
      const sets = details.setScores
        .filter((s) => s.score1 > 0 || s.score2 > 0)
        .map((s) => `H${s.setNumber}: ${s.score1}-${s.score2}`)
        .join(', ');
      if (sets) text += ` (${sets})`;
    }
    if (details.winner === 1) text += ' | Đội 1 thắng';
    else if (details.winner === 2) text += ' | Đội 2 thắng';
    else if (details.winner === 'draw') text += ' | Hòa';

    if (details.mvpAthlete) {
      text += ` | MVP: ${details.mvpAthlete}`;
    }

    if (details.notes && !details.notes.includes('Tỷ số:')) {
      text += `. ${details.notes}`;
    }
    return text;
  },
};
