"""
make_stickman.py - Genera el personaje "Stickman" low-poly para StickMan Arena.

Uso (con el modulo bpy instalado via pip, sin GUI):
    python3 tools/blender/make_stickman.py

Produce:
    Assets/Models/Stickman/stickman.fbx   (o stickman.glb si el exportador FBX no esta)
    docs/previews/stickman-preview.png    (render 640x640, si el render headless funciona)

El script es idempotente: limpia la escena por defecto antes de construir.
"""
import math
import os
import sys

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
# Proporciones (en metros). Ajusta aqui para cambiar el look del personaje.
# --------------------------------------------------------------------------
HEIGHT = 1.8            # altura total aprox. (pies en z=0)
HEAD_R = 0.17           # radio de la cabeza
STROKE_R = 0.045        # grosor del trazo (radio de los cilindros)
HAND_R = 0.065          # radio de manos
FOOT_R = 0.07           # radio de pies
SEGMENTS = 7            # segmentos radiales de los cilindros (6-8)

# Alturas de articulaciones (z) derivadas de HEIGHT
Z_HIP = HEIGHT * 0.50
Z_KNEE = HEIGHT * 0.27
Z_ANKLE = FOOT_R * 0.9
Z_CHEST = HEIGHT * 0.72
Z_SHOULDER = HEIGHT * 0.76
Z_NECK = HEIGHT * 0.80
Z_HEAD = HEIGHT - HEAD_R           # centro de la cabeza
Z_ELBOW = HEIGHT * 0.62
Z_WRIST = HEIGHT * 0.49
SHOULDER_W = 0.20                  # medio ancho de hombros (x)
HIP_W = 0.09                       # medio ancho de cadera (x)
ANKLE_W = 0.11                     # medio ancho de tobillos (x)

# --------------------------------------------------------------------------
# Utilidades
# --------------------------------------------------------------------------
def clear_scene():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    for coll in (bpy.data.meshes, bpy.data.objects, bpy.data.materials,
                 bpy.data.armatures, bpy.data.actions, bpy.data.cameras,
                 bpy.data.lights, bpy.data.images):
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
            bsdf.inputs["Specular IOR Level"].default_value = 0.1
    mat.diffuse_color = rgba
    mat.roughness = roughness
    return mat


def add_capsule(p0, p1, radius, name, segments=SEGMENTS):
    """Cilindro de p0 a p1 con tapas, orientado por el eje."""
    p0, p1 = Vector(p0), Vector(p1)
    axis = p1 - p0
    length = axis.length
    center = (p0 + p1) / 2
    bpy.ops.mesh.primitive_cylinder_add(vertices=segments, radius=radius,
                                        depth=length, location=center,
                                        end_fill_type='NGON')
    obj = bpy.context.active_object
    obj.name = name
    obj.rotation_mode = 'QUATERNION'
    obj.rotation_quaternion = axis.to_track_quat('Z', 'Y')
    return obj


def add_ball(center, radius, name, subdiv=1, scale=(1, 1, 1)):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=subdiv, radius=radius,
                                          location=center)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = scale
    return obj


def join_objects(objs, name):
    bpy.ops.object.select_all(action='DESELECT')
    for o in objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    bpy.ops.object.join()
    merged = bpy.context.active_object
    merged.name = name
    merged.data.name = name
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    return merged


def triangle_count(obj):
    return sum(len(p.vertices) - 2 for p in obj.data.polygons)


# --------------------------------------------------------------------------
# Construccion del mesh
# --------------------------------------------------------------------------
def build_mesh():
    ink = make_material("Ink", (0.01, 0.01, 0.012, 1.0), roughness=1.0)
    eye = make_material("Eye", (1.0, 1.0, 1.0, 1.0), roughness=0.6)

    parts = {}  # nombre_de_parte -> (objeto, hueso)

    # Cabeza
    parts["Head"] = (add_ball((0, 0, Z_HEAD), HEAD_R, "p_Head", subdiv=1), "Head")
    # Cuello
    parts["Neck"] = (add_capsule((0, 0, Z_NECK - 0.02), (0, 0, Z_HEAD - HEAD_R * 0.8),
                                 STROKE_R, "p_Neck"), "Neck")
    # Torso: Hips -> Chest (dos tramos para que doble en Spine)
    parts["Spine"] = (add_capsule((0, 0, Z_HIP - 0.03), (0, 0, Z_CHEST), STROKE_R, "p_Spine"), "Spine")
    parts["Chest"] = (add_capsule((0, 0, Z_CHEST), (0, 0, Z_NECK), STROKE_R, "p_Chest"), "Chest")
    # Hombros (barra horizontal) y cadera
    parts["ChestBar"] = (add_capsule((-SHOULDER_W, 0, Z_SHOULDER), (SHOULDER_W, 0, Z_SHOULDER),
                                     STROKE_R, "p_ChestBar"), "Chest")
    parts["HipBar"] = (add_capsule((-HIP_W, 0, Z_HIP), (HIP_W, 0, Z_HIP), STROKE_R, "p_HipBar"), "Hips")
    parts["ShoulderL"] = (add_ball((SHOULDER_W, 0, Z_SHOULDER), STROKE_R * 1.1, "p_ShL", 0), "Chest")
    parts["ShoulderR"] = (add_ball((-SHOULDER_W, 0, Z_SHOULDER), STROKE_R * 1.1, "p_ShR", 0), "Chest")

    for side, sx in (("L", 1), ("R", -1)):
        # Brazos
        sh = (sx * SHOULDER_W, 0, Z_SHOULDER)
        el = (sx * (SHOULDER_W + 0.06), 0, Z_ELBOW)
        wr = (sx * (SHOULDER_W + 0.09), 0.02, Z_WRIST)
        parts[f"UpperArm{side}"] = (add_capsule(sh, el, STROKE_R, f"p_UpperArm{side}"), f"{side}_UpperArm")
        parts[f"Elbow{side}"] = (add_ball(el, STROKE_R * 1.05, f"p_Elbow{side}", 0), f"{side}_Forearm")
        parts[f"Forearm{side}"] = (add_capsule(el, wr, STROKE_R, f"p_Forearm{side}"), f"{side}_Forearm")
        parts[f"Hand{side}"] = (add_ball((wr[0], wr[1], wr[2] - HAND_R * 0.6), HAND_R, f"p_Hand{side}", 1), f"{side}_Hand")
        # Piernas
        hp = (sx * HIP_W, 0, Z_HIP)
        kn = (sx * (HIP_W + 0.01), 0.0, Z_KNEE)
        an = (sx * ANKLE_W, 0, Z_ANKLE)
        parts[f"Thigh{side}"] = (add_capsule(hp, kn, STROKE_R, f"p_Thigh{side}"), f"{side}_Thigh")
        parts[f"Knee{side}"] = (add_ball(kn, STROKE_R * 1.05, f"p_Knee{side}", 0), f"{side}_Shin")
        parts[f"Shin{side}"] = (add_capsule(kn, an, STROKE_R, f"p_Shin{side}"), f"{side}_Shin")
        parts[f"Foot{side}"] = (add_ball((an[0], 0.05, FOOT_R * 0.75), FOOT_R, f"p_Foot{side}", 1,
                                         scale=(1.0, 1.5, 0.75)), f"{side}_Foot")

    # Ojos (material Eye) -> mirando a -Y (frente de Blender)
    for side, sx in (("L", 1), ("R", -1)):
        parts[f"Eye{side}"] = (add_ball((sx * 0.065, -HEAD_R * 0.86, Z_HEAD + 0.03), 0.035,
                                        f"p_Eye{side}", 1, scale=(1.0, 0.45, 1.0)), "Head")

    # Asignar materiales y grupos de vertices por parte antes de unir
    objs = []
    for key, (obj, bone) in parts.items():
        obj.data.materials.clear()
        obj.data.materials.append(eye if key.startswith("Eye") else ink)
        vg = obj.vertex_groups.new(name=bone)
        vg.add(list(range(len(obj.data.vertices))), 1.0, 'REPLACE')
        objs.append(obj)

    mesh_obj = join_objects(objs, "Stickman")
    # Sombreado suave para que el trazo se lea redondo
    for p in mesh_obj.data.polygons:
        p.use_smooth = True
    return mesh_obj


# --------------------------------------------------------------------------
# Armature
# --------------------------------------------------------------------------
BONES = [
    # name, head, tail, parent, connect
    ("Root", (0, 0, 0), (0, 0.25, 0), None, False),
    ("Hips", (0, 0, Z_HIP), (0, 0, Z_HIP + 0.1), "Root", False),
    ("Spine", (0, 0, Z_HIP + 0.1), (0, 0, Z_CHEST), "Hips", True),
    ("Chest", (0, 0, Z_CHEST), (0, 0, Z_NECK), "Spine", True),
    ("Neck", (0, 0, Z_NECK), (0, 0, Z_HEAD - HEAD_R * 0.7), "Chest", True),
    ("Head", (0, 0, Z_HEAD - HEAD_R * 0.7), (0, 0, HEIGHT), "Neck", True),
]
for side, sx in (("L", 1), ("R", -1)):
    sh = (sx * SHOULDER_W, 0, Z_SHOULDER)
    el = (sx * (SHOULDER_W + 0.06), 0, Z_ELBOW)
    wr = (sx * (SHOULDER_W + 0.09), 0.02, Z_WRIST)
    hp = (sx * HIP_W, 0, Z_HIP)
    kn = (sx * (HIP_W + 0.01), 0.0, Z_KNEE)
    an = (sx * ANKLE_W, 0, Z_ANKLE)
    BONES += [
        (f"{side}_UpperArm", sh, el, "Chest", False),
        (f"{side}_Forearm", el, wr, f"{side}_UpperArm", True),
        (f"{side}_Hand", wr, (wr[0], wr[1], wr[2] - HAND_R * 1.6), f"{side}_Forearm", True),
        (f"{side}_Thigh", hp, kn, "Hips", False),
        (f"{side}_Shin", kn, an, f"{side}_Thigh", True),
        (f"{side}_Foot", an, (an[0], 0.14, 0.0), f"{side}_Shin", True),
    ]


def build_armature():
    arm_data = bpy.data.armatures.new("StickmanRig")
    arm_obj = bpy.data.objects.new("StickmanRig", arm_data)
    bpy.context.scene.collection.objects.link(arm_obj)
    bpy.context.view_layer.objects.active = arm_obj
    arm_obj.select_set(True)
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


def skin(mesh_obj, arm_obj):
    """Intenta pesos automaticos; si fallan, usa los grupos por parte ya creados."""
    mesh_obj.parent = arm_obj
    mode = "automatic"
    try:
        bpy.ops.object.select_all(action='DESELECT')
        mesh_obj.select_set(True)
        arm_obj.select_set(True)
        bpy.context.view_layer.objects.active = arm_obj
        # guardar grupos manuales para restaurar si hace falta
        bpy.ops.object.parent_set(type='ARMATURE_AUTO')
        # comprobar que todos los vertices tienen peso
        unweighted = sum(1 for v in mesh_obj.data.vertices if not any(g.weight > 0.01 for g in v.groups))
        if unweighted > 0:
            raise RuntimeError(f"{unweighted} vertices sin peso")
    except Exception as e:  # noqa
        print(f"[skin] pesos automaticos fallaron ({e}); usando grupos por parte")
        mode = "per-part vertex groups"
        # eliminar modificadores de armature previos y grupos sobrantes
        for m in list(mesh_obj.modifiers):
            if m.type == 'ARMATURE':
                mesh_obj.modifiers.remove(m)
        mesh_obj.parent = arm_obj
        mesh_obj.matrix_parent_inverse = arm_obj.matrix_world.inverted()
    # asegurar modificador de armature
    if not any(m.type == 'ARMATURE' for m in mesh_obj.modifiers):
        mod = mesh_obj.modifiers.new("Armature", 'ARMATURE')
        mod.object = arm_obj
    return mode


# --------------------------------------------------------------------------
# Animaciones (simples, por rotacion de huesos)
# --------------------------------------------------------------------------
def key_pose(arm_obj, action, frame, pose):
    """pose: {bone: (rx, ry, rz grados) | ('loc', (x,y,z))}"""
    bpy.context.scene.frame_set(frame)
    for bone_name, val in pose.items():
        pb = arm_obj.pose.bones[bone_name]
        if isinstance(val, tuple) and val and val[0] == 'loc':
            pb.location = Vector(val[1])
            pb.keyframe_insert("location", frame=frame)
        else:
            pb.rotation_mode = 'XYZ'
            pb.rotation_euler = tuple(math.radians(a) for a in val)
            pb.keyframe_insert("rotation_euler", frame=frame)


def make_action(arm_obj, name, length, keys, cyclic=True):
    action = bpy.data.actions.new(name)
    arm_obj.animation_data_create()
    arm_obj.animation_data.action = action
    # Blender 4.4+/5.x: slots
    try:
        if hasattr(action, "slots") and len(action.slots) == 0:
            slot = action.slots.new(id_type='OBJECT', name=arm_obj.name)
            arm_obj.animation_data.action_slot = slot
    except Exception:
        pass
    action.use_frame_range = True
    action.frame_start, action.frame_end = 1, length
    for frame, pose in keys:
        key_pose(arm_obj, action, frame, pose)
    if cyclic and hasattr(action, "fcurves"):
        for fc in action.fcurves:
            try:
                fc.modifiers.new('CYCLES')
            except Exception:
                pass
    return action


def rest_pose(arm_obj):
    for pb in arm_obj.pose.bones:
        pb.rotation_mode = 'XYZ'
        pb.rotation_euler = (0, 0, 0)
        pb.location = (0, 0, 0)


def build_actions(arm_obj):
    actions = []
    Z = {b: (0, 0, 0) for b in arm_obj.pose.bones.keys()}

    # Idle: balanceo suave, 60 frames
    idle = [
        (1, {**Z}),
        (30, {"Hips": ('loc', (0, -0.02, 0)), "Spine": (3, 0, 0), "Chest": (2, 0, 0), "Head": (-3, 0, 0),
              "L_UpperArm": (0, 0, 3), "R_UpperArm": (0, 0, -3), "L_Thigh": (2, 0, 0), "R_Thigh": (2, 0, 0),
              "L_Shin": (-4, 0, 0), "R_Shin": (-4, 0, 0)}),
        (60, {**Z, "Hips": ('loc', (0, 0, 0))}),
    ]
    actions.append(make_action(arm_obj, "Idle", 60, idle))

    # Run: 24 frames, brazos y piernas alternos
    def run_pose(s):
        return {
            "Hips": ('loc', (0, -0.03 * abs(s), 0)),
            "Spine": (8, 0, 0), "Chest": (4, 0, 0),
            "L_Thigh": (-40 * s, 0, 0), "R_Thigh": (40 * s, 0, 0),
            "L_Shin": (45 * max(0, -s) + 10, 0, 0), "R_Shin": (45 * max(0, s) + 10, 0, 0),
            "L_UpperArm": (35 * s, 0, 0), "R_UpperArm": (-35 * s, 0, 0),
            "L_Forearm": (-50, 0, 0), "R_Forearm": (-50, 0, 0),
        }
    run = [(1, run_pose(1)), (7, run_pose(0)), (13, run_pose(-1)), (19, run_pose(0)), (25, run_pose(1))]
    actions.append(make_action(arm_obj, "Run", 24, run))

    # Punch: 20 frames (brazo derecho), no ciclico
    punch = [
        (1, {**Z}),
        (6, {"Chest": (0, 0, 15), "R_UpperArm": (-40, 0, 0), "R_Forearm": (-80, 0, 0), "L_UpperArm": (-20, 0, 0), "L_Forearm": (-90, 0, 0)}),
        (11, {"Chest": (0, 0, -20), "R_UpperArm": (-95, 0, 0), "R_Forearm": (-5, 0, 0), "L_UpperArm": (-20, 0, 0), "L_Forearm": (-90, 0, 0)}),
        (20, {**Z}),
    ]
    actions.append(make_action(arm_obj, "Punch", 20, punch, cyclic=False))

    # Jump: 30 frames, no ciclico
    jump = [
        (1, {**Z}),
        (7, {"Hips": ('loc', (0, -0.25, 0)), "Spine": (15, 0, 0), "L_Thigh": (-50, 0, 0), "R_Thigh": (-50, 0, 0),
             "L_Shin": (70, 0, 0), "R_Shin": (70, 0, 0), "L_UpperArm": (30, 0, 0), "R_UpperArm": (30, 0, 0)}),
        (16, {"Hips": ('loc', (0, 0.6, 0)), "Spine": (-5, 0, 0), "L_Thigh": (-15, 0, 0), "R_Thigh": (-15, 0, 0),
              "L_Shin": (20, 0, 0), "R_Shin": (20, 0, 0), "L_UpperArm": (-150, 0, 0), "R_UpperArm": (-150, 0, 0)}),
        (25, {"Hips": ('loc', (0, -0.15, 0)), "Spine": (12, 0, 0), "L_Thigh": (-40, 0, 0), "R_Thigh": (-40, 0, 0),
              "L_Shin": (55, 0, 0), "R_Shin": (55, 0, 0), "L_UpperArm": (20, 0, 0), "R_UpperArm": (20, 0, 0)}),
        (30, {**Z, "Hips": ('loc', (0, 0, 0))}),
    ]
    actions.append(make_action(arm_obj, "Jump", 30, jump, cyclic=False))

    # Dejar Idle activo y pose en reposo
    arm_obj.animation_data.action = actions[0]
    bpy.context.scene.frame_set(1)
    rest_pose(arm_obj)
    bpy.context.scene.frame_start, bpy.context.scene.frame_end = 1, 60
    return actions


# --------------------------------------------------------------------------
# Export
# --------------------------------------------------------------------------
def export(mesh_obj, arm_obj, with_anim=True):
    bpy.ops.object.select_all(action='DESELECT')
    mesh_obj.select_set(True)
    arm_obj.select_set(True)
    bpy.context.view_layer.objects.active = arm_obj
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
            print(f"[export] FBX fallo: {e}")
            if with_anim:
                print("[export] reintentando FBX sin animaciones")
                return export(mesh_obj, arm_obj, with_anim=False)
    if hasattr(bpy.ops.export_scene, "gltf"):
        bpy.ops.export_scene.gltf(filepath=glb_path, export_format='GLB', use_selection=True,
                                  export_animations=with_anim, export_yup=True)
        return glb_path, "GLB"
    raise RuntimeError("No hay exportador FBX ni glTF disponible")


# --------------------------------------------------------------------------
# Preview render
# --------------------------------------------------------------------------
def render_preview(target):
    scene = bpy.context.scene
    bpy.ops.object.camera_add(location=(2.6, -2.9, 1.9))
    cam = bpy.context.active_object
    cam.name = "PreviewCam"
    look = Vector((0, 0, HEIGHT * 0.5)) - cam.location
    cam.rotation_euler = look.to_track_quat('-Z', 'Y').to_euler()
    cam.data.lens = 50
    scene.camera = cam

    bpy.ops.object.light_add(type='SUN', location=(2, -2, 4))
    sun = bpy.context.active_object
    sun.rotation_euler = (math.radians(50), 0, math.radians(35))
    sun.data.energy = 3.0

    world = bpy.data.worlds.new("PreviewWorld") if not scene.world else scene.world
    scene.world = world
    world.use_nodes = True
    bg = world.node_tree.nodes.get("Background")
    if bg:
        bg.inputs[0].default_value = (0.86, 0.88, 0.90, 1.0)
        bg.inputs[1].default_value = 1.0

    scene.render.resolution_x = scene.render.resolution_y = 640
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = 'PNG'
    scene.render.filepath = target
    scene.render.film_transparent = False

    # Sin pantalla ni GPU (contenedor en la nube) solo Cycles en CPU funciona: Workbench y EEVEE
    # necesitan EGL y abortan el proceso entero (no es una excepcion capturable).
    # Con STICKMAN_PREVIEW_GPU=1 se prueban tambien los motores que usan GPU.
    engines = ['CYCLES']
    if os.environ.get("STICKMAN_PREVIEW_GPU") == "1":
        engines += ['BLENDER_WORKBENCH', 'BLENDER_EEVEE_NEXT', 'BLENDER_EEVEE']
    for engine in engines:
        try:
            scene.render.engine = engine
            if engine == 'CYCLES':
                scene.cycles.device = 'CPU'
                scene.cycles.samples = 32
                scene.cycles.use_denoising = False
            elif engine == 'BLENDER_WORKBENCH':
                sh = scene.display.shading
                sh.light = 'STUDIO'
                sh.color_type = 'MATERIAL'
                sh.show_shadows = True
                sh.background_type = 'VIEWPORT'
                sh.background_color = (0.86, 0.88, 0.90)
                scene.display.render_aa = '8'
            bpy.ops.render.render(write_still=True)
            if os.path.exists(target) and os.path.getsize(target) > 0:
                return engine
        except Exception as e:
            print(f"[preview] {engine} fallo: {e}")
    return None


# --------------------------------------------------------------------------
# Main
# --------------------------------------------------------------------------
def main():
    clear_scene()
    scene = bpy.context.scene
    scene.unit_settings.system = 'METRIC'
    scene.unit_settings.scale_length = 1.0
    scene.render.fps = 30

    mesh_obj = build_mesh()
    tris = triangle_count(mesh_obj)
    arm_obj = build_armature()
    skin_mode = skin(mesh_obj, arm_obj)

    actions, anim_err = [], None
    try:
        actions = build_actions(arm_obj)
    except Exception as e:
        anim_err = str(e)
        print(f"[anim] fallo creando acciones: {e}")

    out_path, fmt = export(mesh_obj, arm_obj, with_anim=bool(actions))

    dims = mesh_obj.dimensions
    print("\n================ RESUMEN STICKMAN ================")
    print(f"Blender bpy       : {bpy.app.version_string}")
    print(f"Triangulos        : {tris}  (vertices: {len(mesh_obj.data.vertices)})")
    print(f"Dimensiones (m)   : x={dims.x:.2f} y={dims.y:.2f} z={dims.z:.2f}")
    print(f"Huesos ({len(arm_obj.data.bones)})      : {', '.join(b.name for b in arm_obj.data.bones)}")
    print(f"Skinning          : {skin_mode}")
    if actions:
        print(f"Acciones ({len(actions)})      : " + ", ".join(f"{a.name}[{int(a.frame_range[0])}-{int(a.frame_range[1])}]" for a in actions))
    else:
        print(f"Acciones          : ninguna ({anim_err})")
    print(f"Modelo ({fmt})      : {out_path}  ({os.path.getsize(out_path)/1024:.1f} KB)")
    print("==================================================", flush=True)

    if os.environ.get("STICKMAN_SKIP_PREVIEW") == "1":
        print("Preview           : omitido (STICKMAN_SKIP_PREVIEW=1)")
        return
    try:
        engine = render_preview(OUT_PREVIEW)
    except Exception as e:
        print(f"[preview] render fallo: {e}")
        engine = None
    if engine and os.path.exists(OUT_PREVIEW):
        print(f"Preview ({engine}): {OUT_PREVIEW}  ({os.path.getsize(OUT_PREVIEW)/1024:.1f} KB)")
    else:
        print("Preview           : NO generado (render headless fallo)")


if __name__ == "__main__":
    main()
