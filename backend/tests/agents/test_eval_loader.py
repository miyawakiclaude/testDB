from pathlib import Path

import yaml


def test_eval_scenarios_load() -> None:
    path = Path(__file__).resolve().parents[2] / "data" / "care_eval_scenarios.yaml"
    data = yaml.safe_load(path.read_text(encoding="utf-8"))
    assert "scenarios" in data
    assert len(data["scenarios"]) >= 10
    seen_ids: set[str] = set()
    for s in data["scenarios"]:
        assert s["id"] not in seen_ids, f"duplicate id: {s['id']}"
        seen_ids.add(s["id"])
        assert s["agent"] in {
            "situation_summarizer",
            "doctor_pack_generator",
            "conflict_mediator",
            "decision_helper",
            "task_balancer",
            "orchestrator",
        }
        assert "inputs" in s
        assert "expectations" in s
