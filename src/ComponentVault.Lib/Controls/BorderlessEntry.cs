namespace ComponentVault.Lib.Controls;

// Controles nativos sem o adorno da plataforma (sublinhado no Android, caixa no Windows).
// A moldura vem de quem os envolve (VaultTextField, FieldFrame). O handler é ajustado em
// UseVaultDesignSystem e só atinge estes tipos, nunca o Entry/Picker comum.

public class BorderlessEntry : Entry
{
}

public class BorderlessEditor : Editor
{
}

public class BorderlessPicker : Picker
{
}

public class BorderlessDatePicker : DatePicker
{
}

public class BorderlessTimePicker : TimePicker
{
}
