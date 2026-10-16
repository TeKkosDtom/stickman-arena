"""
make_stickman.py - Genera el personaje "Stickman" Low Poly de StickMan Arena con Blender.

Flujo de modelado (el mismo que haría una persona en Blender, pero por script):
  1. Esqueleto de vértices y aristas (un "palito" por hueso).
  2. Modificador Skin → tubo continuo con articulaciones redondeadas; radio por vértice.
  3. Subdivisión (1 nivel) para suavizar y Decimate para entrar en el presupuesto Low Poly.
  4. Cabeza (esfera UV) unida a la malla; ojos y sonrisa con material blanco.
  5. Armature con huesos nombrados y pesos automáticos (heat weighting), con respaldo propio.
  6. Cuatro acciones: Idle, Run, Punch, Jump.
  7. Exporta FBX para Unity y renderiza una hoja de vistas previas con Cycles (CPU).

Uso (módulo bpy instalado vía pip, sin GUI):
    python3 tools/blender/make_stickman.py
    STICKMAN_SKIP_PREVIEW=1 python3 tools/blender/make_stickman.py   # sin render

Produce:
    Assets/Models/Stickman/stickman.fbx
    docs/previews/stickman-preview.png
"""
import math
import os

import bpy
from mathutils import Vector

# --------------------------------------------------------------------------
# Rutas
# --------------------------------------------------------------------------
HERE = os.path.dirname(os.path.abspath(__file__))
PROJECT = os.path.abspath(os.path.join(HERE, "..", ".."))
OUT_MODEL_DIR = os.path.join(PROJECT, "Assets", "Models", "Stickman")
OUT_PREVIEW = os.path.join(PROJECT, "docs", "previews", "stickman-preview.png")
os.makedirs(OUT_MODEL_DIR, exist_ok=True)
os.makedirs(os.path.dirname(OUT_PREVIEW), exist_ok=True)

# --------------------------------------------------------------------------
# Proporciones (metros, pies en z=0, frente hacia -Y como manda Blender).
# Referencia: stickman clásico (cabeza grande, trazo grueso ~1/3 de la cabeza,
# manos y pies redondos, sin articulaciones marcadas).
# --------------------------------------------------------------------------
HEIGHT = 1.8
HEAD_R = 0.21
STROKE = 0.068          # radio del trazo
HAND_R = 0.085
FOOT_R = 0.08
TRI_BUDGET = 900        # presupuesto total del personaje (regla: < 1.000 triángulos)

Z_HIP = HEIGHT * 0.50
Z_KNEE = HEIGHT * 0.27
Z_ANKLE = 0.10
Z_CHEST = HEIGHT * 0.72
Z_SHOULDER = HEIGHT * 0.75
Z_NECK = HEIGHT * 0.79
Z_HEAD = HEIGHT - HEAD_R
Z_ELBOW = HEIGHT * 0.61
Z_WRIST = HEIGHT * 0.48
SHOULDER_W = 0.20
HIP_W = 0.09
ANKLE_W = 0.12

# Articulaciones: nombre -> (posición, radio del skin)
JOINTS = {
    "Hips": ((0, 0, Z_HIP), STROKE * 1.05),
    "Chest": ((0, 0, Z_CHEST), STROKE * 1.1),
    "Neck": ((0, 0, Z_NECK), STROKE * 0.9),
    "NeckTop": ((0, 0, Z_HEAD - HEAD_R * 0.6), STROKE * 0.8),
}
for side, sx in (("L", 1), ("R", -1)):
    JOINTS[f"{side}_Shoulder"] = ((sx * SHOULDER_W, 0, Z_SHOULDER), STROKE)
    JOINTS[f"{side}_Elbow"] = ((sx * (SHOULDER_W + 0.05), 0.0, Z_ELBOW), STROKE * 0.95)
    JOINTS[f"{side}_Wrist"] = ((sx * (SHOULDER_W + 0.08), -0.02, Z_WRIST), STROKE * 0.95)
    JOINTS[f"{side}_HandEnd"] = ((sx * (SHOULDER_W + 0.09), -0.03, Z_WRIST - 0.11), HAND_R)
    JOINTS[f"{side}_Hip"] = ((sx * HIP_W, 0, Z_HIP - 0.02), STROKE)
    JOINTS[f"{side}_Knee"] = ((sx * (HIP_W + 0.015), 0.0, Z_KNEE), STROKE * 0.95)
    JOINTS[f"{side}_Ankle"] = ((sx * ANKLE_W, 0.01, Z_ANKLE), STROKE * 0.95)
    JOINTS[f"{side}_Toe"] = ((sx * ANKLE_W, -0.15, FOOT_R * 0.9), FOOT_R)

# Aristas del esqueleto (cada una se vuelve un tubo con Skin)
EDGES = [("Hips", "Chest"), ("Chest", "Neck"), ("Neck", "NeckTop")]
for side in ("L", "R"):
    EDGES += [("Chest", f"{side}_Shoulder"), (f"{side}_Shoulder", f"{side}_Elbow"),
              (f"{side}_Elbow", f"{side}_Wrist"), (f"{side}_Wrist", f"{side}_HandEnd"),
              ("Hips", f"{side}_Hip"), (f"{side}_Hip", f"{side}_Knee"),
              (f"{side}_Knee", f"{side}_Ankle"), (f"{side}_Ankle", f"{side}_Toe")]

# Huesos: nombre, cabeza, cola, padre, conectado
BONES = [
    ("Root", (0, 0, 0), (0, 0.25, 0), None, False),
    ("Hips", (0, 0, Z_HIP - 0.05), (0, 0, Z_HIP + 0.08), "Root", False),
    ("Spine", (0, 0, Z_HIP + 0.08), (0, 0, Z_CHEST), "Hips", True),
    ("Chest", (0, 0, Z_CHEST), (0, 0, Z_NECK), "Spine", True),
    ("Neck", (0, 0, Z_NECK), (0, 0, Z_HEAD - HEAD_R * 0.6), "Chest", True),
    ("Head", (0, 0, Z_HEAD - HEAD_R * 0.6), (0, 0, HEIGHT), "Neck", True),
]
for side in ("L", "R"):
    j = lambda n: JOINTS[f"{side}_{n}"][0]
    BONES += [
        (f"{side}_UpperArm", j("Shoulder"), j("Elbow"), "Chest", False),
        (f"{side}_Forearm", j("Elbow"), j("Wrist"), f"{side}_UpperArm", True),
        (f"{side}_Hand", j("Wrist"), j("HandEnd"), f"{side}_Forearm", True),
        (f"{side}_Thigh", j("Hip"), j("Knee"), "Hips", False),
        (f"{side}_Shin", j("Knee"), j("Ankle"), f"{side}_Thigh", True),
        (f"{side}_Foot", j("Ankle"), j("Toe"), f"{side}_Shin", True),
    ]


# --------------------------------------------------------------------------
# Utilidades
# --------------------------------------------------------------------------
def clear_scene():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    for coll in (bpy.data.meshes, bpy.data.objects, bpy.data.materials, bpy.data.armatures,
                 bpy.data.actions, bpy.data.cameras, bpy.data.lights, bpy.data.images):
        for block in list(coll):
            try:
                coll.remove(block)
            except Exception:
                pass


def make_material(name, rgba, roughness=1.0):
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = rgba
        bsdf.inputs["Roughness"].default_value = roughness
        if "Specular IOR Level" in bsdf.inputs:
            bsdf.inputs["Specular IOR Level"].default_value = 0.15
    mat.diffuse_color = rgba
    mat.roughness = roughness
    return mat


def select_only(objs, active=None):
    bpy.ops.object.select_all(action='DESELECT')
    for o in objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = active or objs[0]


def apply_modifiers(obj):
    select_only([obj])
    for m in list(obj.modifiers):
        bpy.ops.object.modifier_apply(modifier=m.name)


def triangle_count(obj):
    return sum(len(p.vertices) - 2 for p in obj.data.polygons)


def shade_smooth(obj):
    for p in obj.data.polygons:
        p.use_smooth = True


# --------------------------------------------------------------------------
# Cuerpo: esqueleto de aristas + Skin + Subdivisión + Decimate
# --------------------------------------------------------------------------
def build_body(ink):
    names = list(JOINTS.keys())
    index = {n: i for i, n in enumerate(names)}
    verts = [JOINTS[n][0] for n in names]
    edges = [(index[a], index[b]) for a, b in EDGES]

    me = bpy.data.meshes.new("StickmanBody")
    me.from_pydata(verts, edges, [])
    me.update()
    body = bpy.data.objects.new("StickmanBody", me)
    bpy.context.scene.collection.objects.link(body)
    select_only([body])

    skin = body.modifiers.new("Skin", 'SKIN')
    skin.use_smooth_shade = True
    skin.branch_smoothing = 0.6   # hombros y cadera redondeados, sin bultos
    # El modificador crea la capa de datos; radio por articulación y raíz en la cadera
    layer = me.skin_vertices[0].data
    for n, (_, r) in JOINTS.items():
        layer[index[n]].radius = (r, r)
    layer[index["Hips"]].use_root = True

    sub = body.modifiers.new("Subdivision", 'SUBSURF')
    sub.levels = 1
    sub.render_levels = 1
    apply_modifiers(body)

    # Cabeza: esfera UV unida al cuerpo
    bpy.ops.mesh.primitive_uv_sphere_add(segments=14, ring_count=9, radius=HEAD_R,
                                         location=(0, 0, Z_HEAD))
    head = bpy.context.active_object
    select_only([body, head], active=body)
    bpy.ops.object.join()
    body = bpy.context.active_object
    body.name = "Stickman"
    body.data.name = "Stickman"

    # Decimate para entrar en presupuesto (dejando margen para ojos y sonrisa)
    tris = triangle_count(body)
    target = TRI_BUDGET - 230   # margen para ojos y sonrisa
    if tris > target:
        dec = body.modifiers.new("Decimate", 'DECIMATE')
        dec.decimate_type = 'COLLAPSE'
        dec.ratio = target / tris
        dec.use_collapse_triangulate = True
        apply_modifiers(body)
    shade_smooth(body)

    body.data.materials.clear()
    body.data.materials.append(ink)
    return body


def build_face(eye_mat):
    """Ojos pequeños y sonrisa blanca sobre la cara (-Y). Devuelve los objetos."""
    parts = []
    for sx in (1, -1):
        bpy.ops.mesh.primitive_uv_sphere_add(segments=8, ring_count=5, radius=0.03,
                                             location=(sx * 0.075, -HEAD_R * 0.92, Z_HEAD + 0.05))
        eye = bpy.context.active_object
        eye.scale = (1.0, 0.4, 1.3)
        parts.append(eye)
    # Sonrisa: arco de pequeños cilindros pegado a la esfera de la cabeza
    n = 6
    r_arc = HEAD_R * 0.55
    pts = []
    for i in range(n + 1):
        a = math.radians(-55 + 110 * i / n)
        x = math.sin(a) * r_arc
        z = Z_HEAD - 0.115 + (1 - math.cos(a)) * r_arc * 0.9   # extremos arriba = sonrisa
        dz = z - Z_HEAD
        y = -math.sqrt(max(HEAD_R ** 2 - x ** 2 - dz ** 2, 1e-4)) - 0.004
        pts.append(Vector((x, y, z)))
    for i in range(n):
        p0, p1 = pts[i], pts[i + 1]
        axis = p1 - p0
        bpy.ops.mesh.primitive_cylinder_add(vertices=5, radius=0.017, depth=axis.length + 0.012,
                                            location=(p0 + p1) / 2, end_fill_type='NGON')
        seg = bpy.context.active_object
        seg.rotation_mode = 'QUATERNION'
        seg.rotation_quaternion = axis.to_track_quat('Z', 'Y')
        parts.append(seg)
    for o in parts:
        o.data.materials.clear()
        o.data.materials.append(eye_mat)
    select_only(parts)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    return parts


# --------------------------------------------------------------------------
# Armature y pesos
# --------------------------------------------------------------------------
def build_armature():
    arm_data = bpy.data.armatures.new("StickmanRig")
    arm_obj = bpy.data.objects.new("StickmanRig", arm_data)
    bpy.context.scene.collection.objects.link(arm_obj)
    select_only([arm_obj])
    bpy.ops.object.mode_set(mode='EDIT')
    eb = arm_data.edit_bones
    for name, head, tail, parent, connect in BONES:
        b = eb.new(name)
        b.head, b.tail = Vector(head), Vector(tail)
        if parent:
            b.parent = eb[parent]
            b.use_connect = connect
    bpy.ops.object.mode_set(mode='OBJECT')
    return arm_obj


def _closest_point_on_segment(p, a, b):
    ab = b - a
    t = 0.0 if ab.length_squared == 0 else max(0.0, min(1.0, (p - a).dot(ab) / ab.length_squared))
    return a + ab * t


def weight_by_nearest_bones(mesh_obj, arm_obj):
    """Respaldo sin heat weighting: cada vértice se reparte entre sus dos huesos más
    cercanos (inverso de la distancia), lo que da articulaciones suaves."""
    for vg in list(mesh_obj.vertex_groups):
        mesh_obj.vertex_groups.remove(vg)
    groups = {b.name: mesh_obj.vertex_groups.new(name=b.name) for b in arm_obj.data.bones if b.name != "Root"}
    segs = [(b.name, Vector(b.head_local), Vector(b.tail_local)) for b in arm_obj.data.bones if b.name != "Root"]
    for v in mesh_obj.data.vertices:
        p = v.co
        dists = sorted(((p - _closest_point_on_segment(p, a, b)).length, n) for n, a, b in segs)
        d0, n0 = dists[0]
        d1, n1 = dists[1]
        if d1 > d0 * 1.6 or d1 - d0 > STROKE:
            groups[n0].add([v.index], 1.0, 'REPLACE')
        else:
            w0 = 1.0 / (d0 + 1e-4)
            w1 = 1.0 / (d1 + 1e-4)
            s = w0 + w1
            groups[n0].add([v.index], w0 / s, 'REPLACE')
            groups[n1].add([v.index], w1 / s, 'REPLACE')


def skin_to_rig(mesh_obj, arm_obj):
    """Pesos automáticos (heat weighting), con respaldo por hueso más cercano."""
    mode = "automatic (heat weighting)"
    try:
        select_only([mesh_obj, arm_obj], active=arm_obj)
        bpy.ops.object.parent_set(type='ARMATURE_AUTO')
        unweighted = sum(1 for v in mesh_obj.data.vertices if not any(g.weight > 0.01 for g in v.groups))
        if unweighted > 0:
            raise RuntimeError(f"{unweighted} vértices sin peso")
    except Exception as e:  # noqa
        print(f"[skin] pesos automáticos fallaron ({e}); usando hueso más cercano")
        mode = "nearest-bone fallback"
        for m in list(mesh_obj.modifiers):
            if m.type == 'ARMATURE':
                mesh_obj.modifiers.remove(m)
        mesh_obj.parent = arm_obj
        mesh_obj.matrix_parent_inverse = arm_obj.matrix_world.inverted()
        weight_by_nearest_bones(mesh_obj, arm_obj)
    if not any(m.type == 'ARMATURE' for m in mesh_obj.modifiers):
        mod = mesh_obj.modifiers.new("Armature", 'ARMATURE')
        mod.object = arm_obj
    return mode


def attach_face(body, face_parts, arm_obj):
    """Une ojos y sonrisa al cuerpo con peso total en el hueso Head."""
    for o in face_parts:
        vg = o.vertex_groups.new(name="Head")
        vg.add(list(range(len(o.data.vertices))), 1.0, 'REPLACE')
    select_only([body] + face_parts, active=body)
    bpy.ops.object.join()
    return bpy.context.active_object


# --------------------------------------------------------------------------
# Animaciones. Convención: rx positivo = balancear la extremidad hacia el frente (-Y),
# sea cual sea la orientación del hueso; FORWARD_SIGN lo resuelve midiendo cada hueso.
# --------------------------------------------------------------------------
FORWARD_SIGN = {}


def compute_forward_signs(arm_obj):
    FORWARD_SIGN.clear()
    for pb in arm_obj.pose.bones:
        pb.rotation_mode = 'XYZ'
        pb.rotation_euler = (0, 0, 0)
    bpy.context.view_layer.update()
    rest_tail = {pb.name: (arm_obj.matrix_world @ pb.tail).copy() for pb in arm_obj.pose.bones}
    for pb in arm_obj.pose.bones:
        pb.rotation_euler = (math.radians(20), 0, 0)
        bpy.context.view_layer.update()
        dy = (arm_obj.matrix_world @ pb.tail).y - rest_tail[pb.name].y
        FORWARD_SIGN[pb.name] = 1 if dy <= 0 else -1
        pb.rotation_euler = (0, 0, 0)
    bpy.context.view_layer.update()


def key_pose(arm_obj, frame, pose):
    """pose: {hueso: (rx, ry, rz grados) | ('loc', (x, y, z))}; loc Y = a lo largo del hueso."""
    bpy.context.scene.frame_set(frame)
    for bone_name, val in pose.items():
        pb = arm_obj.pose.bones[bone_name]
        if isinstance(val, tuple) and val and val[0] == 'loc':
            pb.location = Vector(val[1])
            pb.keyframe_insert("location", frame=frame)
        else:
            pb.rotation_mode = 'XYZ'
            rx, ry, rz = val
            rx *= FORWARD_SIGN.get(bone_name, 1)
            pb.rotation_euler = (math.radians(rx), math.radians(ry), math.radians(rz))
            pb.keyframe_insert("rotation_euler", frame=frame)


def make_action(arm_obj, name, length, keys):
    action = bpy.data.actions.new(name)
    arm_obj.animation_data_create()
    arm_obj.animation_data.action = action
    if hasattr(action, "slots") and len(action.slots) == 0:   # Blender 4.4+/5.x
        slot = action.slots.new(id_type='OBJECT', name=arm_obj.name)
        arm_obj.animation_data.action_slot = slot
    action.use_frame_range = True
    action.frame_start, action.frame_end = 1, length
    for frame, pose in keys:
        key_pose(arm_obj, frame, pose)
    return action


def rest_pose(arm_obj):
    for pb in arm_obj.pose.bones:
        pb.rotation_mode = 'XYZ'
        pb.rotation_euler = (0, 0, 0)
        pb.location = (0, 0, 0)


def build_actions(arm_obj):
    actions = []
    compute_forward_signs(arm_obj)
    Z = {b: (0, 0, 0) for b in arm_obj.pose.bones.keys()}

    # Idle (60 f): respiración y balanceo leve
    idle = [
        (1, {**Z}),
        (30, {"Hips": ('loc', (0, -0.015, 0)), "Spine": (3, 0, 0), "Chest": (2, 0, 0), "Head": (-3, 0, 0),
              "L_UpperArm": (0, 0, 3), "R_UpperArm": (0, 0, -3), "L_Thigh": (2, 0, 0), "R_Thigh": (2, 0, 0),
              "L_Shin": (-4, 0, 0), "R_Shin": (-4, 0, 0)}),
        (60, {**Z, "Hips": ('loc', (0, 0, 0))}),
    ]
    actions.append(make_action(arm_obj, "Idle", 60, idle))

    # Run (24 f): zancada amplia como el stickman clásico, brazos opuestos a las piernas
    def run_pose(s, bounce):
        return {
            "Hips": ('loc', (0, -0.04 * bounce, 0)),
            "Spine": (10, 0, 0), "Chest": (4, 0, 0), "Head": (-6, 0, 0),
            "L_Thigh": (45 * s, 0, 0), "R_Thigh": (-45 * s, 0, 0),
            "L_Shin": (-(55 * max(0, -s) + 12), 0, 0), "R_Shin": (-(55 * max(0, s) + 12), 0, 0),
            "L_Foot": (10 * s, 0, 0), "R_Foot": (-10 * s, 0, 0),
            "L_UpperArm": (-40 * s, 0, 6), "R_UpperArm": (40 * s, 0, -6),
            "L_Forearm": (60, 0, 0), "R_Forearm": (60, 0, 0),
        }
    run = [(1, run_pose(1, 0)), (7, run_pose(0, 1)), (13, run_pose(-1, 0)), (19, run_pose(0, 1)), (25, run_pose(1, 0))]
    actions.append(make_action(arm_obj, "Run", 24, run))

    # Punch (20 f): guardia, golpe derecho con giro del pecho, vuelta
    punch = [
        (1, {**Z}),
        (6, {"Chest": (0, 0, 15), "R_UpperArm": (40, 0, 0), "R_Forearm": (85, 0, 0),
             "L_UpperArm": (25, 0, 0), "L_Forearm": (95, 0, 0)}),
        (11, {"Chest": (0, 0, -25), "R_UpperArm": (95, 0, 0), "R_Forearm": (5, 0, 0),
              "L_UpperArm": (25, 0, 0), "L_Forearm": (95, 0, 0)}),
        (20, {**Z}),
    ]
    actions.append(make_action(arm_obj, "Punch", 20, punch))

    # Jump (30 f): agacharse, saltar con brazos arriba, aterrizar
    jump = [
        (1, {**Z}),
        (7, {"Hips": ('loc', (0, -0.22, 0)), "Spine": (15, 0, 0), "L_Thigh": (55, 0, 0), "R_Thigh": (55, 0, 0),
             "L_Shin": (-75, 0, 0), "R_Shin": (-75, 0, 0), "L_UpperArm": (-30, 0, 0), "R_UpperArm": (-30, 0, 0)}),
        (16, {"Hips": ('loc', (0, 0.6, 0)), "Spine": (-5, 0, 0), "L_Thigh": (20, 0, 0), "R_Thigh": (20, 0, 0),
              "L_Shin": (-25, 0, 0), "R_Shin": (-25, 0, 0), "L_UpperArm": (160, 0, 10), "R_UpperArm": (160, 0, -10)}),
        (25, {"Hips": ('loc', (0, -0.14, 0)), "Spine": (12, 0, 0), "L_Thigh": (45, 0, 0), "R_Thigh": (45, 0, 0),
              "L_Shin": (-60, 0, 0), "R_Shin": (-60, 0, 0), "L_UpperArm": (-20, 0, 0), "R_UpperArm": (-20, 0, 0)}),
        (30, {**Z, "Hips": ('loc', (0, 0, 0))}),
    ]
    actions.append(make_action(arm_obj, "Jump", 30, jump))

    arm_obj.animation_data.action = actions[0]
    bpy.context.scene.frame_set(1)
    rest_pose(arm_obj)
    bpy.context.scene.frame_start, bpy.context.scene.frame_end = 1, 60
    return actions


# --------------------------------------------------------------------------
# Export
# --------------------------------------------------------------------------
def export(mesh_obj, arm_obj, with_anim=True):
    select_only([mesh_obj, arm_obj], active=arm_obj)
    fbx_path = os.path.join(OUT_MODEL_DIR, "stickman.fbx")
    glb_path = os.path.join(OUT_MODEL_DIR, "stickman.glb")
    if hasattr(bpy.ops.export_scene, "fbx"):
        try:
            bpy.ops.export_scene.fbx(
                filepath=fbx_path, use_selection=True,
                object_types={'ARMATURE', 'MESH'},
                apply_scale_options='FBX_SCALE_ALL', apply_unit_scale=True,
                global_scale=1.0, axis_forward='-Z', axis_up='Y',
                add_leaf_bones=False, bake_anim=with_anim,
                bake_anim_use_all_actions=with_anim, bake_anim_use_nla_strips=False,
                bake_anim_use_all_bones=True, bake_anim_force_startend_keying=True,
                bake_anim_simplify_factor=0.0,
                use_armature_deform_only=True, mesh_smooth_type='FACE',
                use_mesh_modifiers=False, path_mode='STRIP', embed_textures=False,
            )
            return fbx_path, "FBX"
        except Exception as e:
            print(f"[export] FBX falló: {e}")
            if with_anim:
                print("[export] reintentando FBX sin animaciones")
                return export(mesh_obj, arm_obj, with_anim=False)
    if hasattr(bpy.ops.export_scene, "gltf"):
        bpy.ops.export_scene.gltf(filepath=glb_path, export_format='GLB', use_selection=True,
                                  export_animations=with_anim, export_yup=True)
        return glb_path, "GLB"
    raise RuntimeError("No hay exportador FBX ni glTF disponible")


# --------------------------------------------------------------------------
# Vista previa: hoja con tres poses (Cycles CPU; Workbench/EEVEE necesitan GPU y abortan sin EGL)
# --------------------------------------------------------------------------
def render_preview(target, arm_obj):
    scene = bpy.context.scene
    scene.render.engine = 'CYCLES'
    scene.cycles.device = 'CPU'
    scene.cycles.samples = 32
    scene.cycles.use_denoising = False
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = 'PNG'
    scene.render.film_transparent = False

    world = scene.world or bpy.data.worlds.new("PreviewWorld")
    scene.world = world
    world.use_nodes = True
    bg = world.node_tree.nodes.get("Background")
    if bg:
        bg.inputs[0].default_value = (1.0, 0.62, 0.28, 1.0)   # fondo cálido como la referencia
        bg.inputs[1].default_value = 1.0

    bpy.ops.object.light_add(type='SUN', location=(2, -3, 5))
    sun = bpy.context.active_object
    sun.rotation_euler = (math.radians(45), 0, math.radians(20))
    sun.data.energy = 2.5

    bpy.ops.object.camera_add(location=(0, 0, 0))
    cam = bpy.context.active_object
    cam.data.lens = 45
    scene.camera = cam

    shots = [
        ("Idle", 1, (0.6, -4.2, 1.3)),      # frente
        ("Run", 1, (3.0, -3.0, 1.4)),       # 3/4 corriendo
        ("Jump", 16, (4.8, 0.0, 2.2)),      # perfil en el aire
    ]
    files = []
    scratch = os.path.join(os.path.dirname(target), ".tmp")
    os.makedirs(scratch, exist_ok=True)
    for action_name, frame, loc in shots:
        act = bpy.data.actions.get(action_name)
        if act and arm_obj.animation_data:
            arm_obj.animation_data.action = act
        scene.frame_set(frame)
        cam.location = Vector(loc)
        look = Vector((0, 0, HEIGHT * (0.75 if action_name == 'Jump' else 0.5))) - cam.location
        cam.rotation_euler = look.to_track_quat('-Z', 'Y').to_euler()
        scene.render.resolution_x, scene.render.resolution_y = 480, 640
        path = os.path.join(scratch, f"{action_name}.png")
        scene.render.filepath = path
        bpy.ops.render.render(write_still=True)
        files.append(path)

    # Componer la hoja con la API de imágenes de Blender (sin dependencias externas)
    sheet = bpy.data.images.new("Sheet", width=480 * len(files), height=640)
    pixels = [0.0] * (480 * len(files) * 640 * 4)
    for i, path in enumerate(files):
        img = bpy.data.images.load(path)
        px = list(img.pixels)
        w, h = img.size
        for y in range(h):
            src = (y * w) * 4
            dst = (y * 480 * len(files) + i * 480) * 4
            pixels[dst:dst + w * 4] = px[src:src + w * 4]
        bpy.data.images.remove(img)
        os.remove(path)
    sheet.pixels = pixels
    sheet.filepath_raw = target
    sheet.file_format = 'PNG'
    sheet.save()
    try:
        os.rmdir(scratch)
    except OSError:
        pass
    # volver a la pose de reposo
    if arm_obj.animation_data:
        arm_obj.animation_data.action = bpy.data.actions.get("Idle")
    scene.frame_set(1)
    return 'CYCLES'


# --------------------------------------------------------------------------
# Main
# --------------------------------------------------------------------------
def main():
    clear_scene()
    scene = bpy.context.scene
    scene.unit_settings.system = 'METRIC'
    scene.unit_settings.scale_length = 1.0
    scene.render.fps = 30

    ink = make_material("Ink", (0.01, 0.01, 0.012, 1.0), roughness=0.9)
    eye = make_material("Eye", (1.0, 1.0, 1.0, 1.0), roughness=0.6)

    body = build_body(ink)
    arm_obj = build_armature()
    skin_mode = skin_to_rig(body, arm_obj)
    face = build_face(eye)
    body = attach_face(body, face, arm_obj)
    tris = triangle_count(body)

    actions, anim_err = [], None
    try:
        actions = build_actions(arm_obj)
    except Exception as e:
        anim_err = str(e)
        print(f"[anim] falló creando acciones: {e}")

    out_path, fmt = export(body, arm_obj, with_anim=bool(actions))

    dims = body.dimensions
    print("\n================ RESUMEN STICKMAN ================")
    print(f"Blender bpy       : {bpy.app.version_string}")
    print(f"Triángulos        : {tris}  (vértices: {len(body.data.vertices)}, presupuesto {TRI_BUDGET})")
    print(f"Dimensiones (m)   : x={dims.x:.2f} y={dims.y:.2f} z={dims.z:.2f}")
    print(f"Huesos ({len(arm_obj.data.bones)})      : {', '.join(b.name for b in arm_obj.data.bones)}")
    print(f"Skinning          : {skin_mode}")
    if actions:
        print(f"Acciones ({len(actions)})      : " + ", ".join(f"{a.name}[{int(a.frame_range[0])}-{int(a.frame_range[1])}]" for a in actions))
    else:
        print(f"Acciones          : ninguna ({anim_err})")
    print(f"Modelo ({fmt})      : {out_path}  ({os.path.getsize(out_path) / 1024:.1f} KB)")
    print("==================================================", flush=True)

    if os.environ.get("STICKMAN_SKIP_PREVIEW") == "1":
        print("Preview           : omitida (STICKMAN_SKIP_PREVIEW=1)")
        return
    try:
        engine = render_preview(OUT_PREVIEW, arm_obj)
    except Exception as e:
        print(f"[preview] render falló: {e}")
        engine = None
    if engine and os.path.exists(OUT_PREVIEW):
        print(f"Preview ({engine}): {OUT_PREVIEW}  ({os.path.getsize(OUT_PREVIEW) / 1024:.1f} KB)")
    else:
        print("Preview           : NO generada (render sin pantalla falló)")


if __name__ == "__main__":
    main()
