ls -la /var/cynara/db
file /var/cynara/db/* 2>/dev/null || true
head -n 20 /var/cynara/db/* 2>/dev/null || true
