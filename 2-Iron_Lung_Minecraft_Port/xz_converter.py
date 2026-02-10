
x, z, yaw = input("> ").split()

out_x = (float(z) - 5) * 4
out_z = (float(x) - 4) * 4
out_yaw = float(yaw)

if (out_yaw < 0):
    out_yaw = -out_yaw
elif (out_yaw > 0):
    out_yaw = 360 - out_yaw

print(f"{int(out_x)}, {int(out_z)}, {int(out_yaw)}")