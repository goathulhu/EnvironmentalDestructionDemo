shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_burley, specular_schlick_ggx;

uniform vec4 color : source_color = vec4(0.0,0.0,0.0,1.0);
uniform sampler2D texture_albedo : source_color, filter_nearest, repeat_enable;
uniform sampler2D texture_metallic : hint_default_white, filter_nearest, repeat_enable;
uniform sampler2D texture_normal : hint_roughness_normal, filter_nearest, repeat_enable;
uniform sampler2D texture_roughness : hint_roughness_r, filter_nearest, repeat_enable;

uniform float viewmodel_fov = 90.0f;

void vertex()
{
	float onetanfov = 1.0f / tan(0.5f * (viewmodel_fov * PI / 180.0f));
	float aspect = VIEWPORT_SIZE.x / VIEWPORT_SIZE.y;
	PROJECTION_MATRIX[1][1] = -onetanfov;
	PROJECTION_MATRIX[0][0] = onetanfov / aspect;
	POSITION = PROJECTION_MATRIX * MODELVIEW_MATRIX * vec4(VERTEX.xyz, 1.0);
	POSITION.z = mix(POSITION.z, 0.999, 0.1);
}

void fragment()
{
	ALBEDO = color.rgb;//texture(texture_albedo, UV).rgb;
	//METALLIC = texture(texture_metallic, UV).x;
	//NORMAL_MAP = texture(texture_normal, UV).rgb;
	//ROUGHNESS = texture(texture_roughness, UV).x;
}
