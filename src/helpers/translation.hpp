#pragma once

#include <libintl.h>
#include <locale.h>

#define _(String) gettext(String)

// Allows to translate with context
#define GETTEXT_CONTEXT_GLUE "\004"
#define pgettext(Ctxt, String) pgettext_aux(Ctxt GETTEXT_CONTEXT_GLUE String, String)

static const char *pgettext_aux (const char *msg_ctxt_id, const char *msgid)
{
    const char *translation = dcgettext (GETTEXT_PACKAGE, msg_ctxt_id, LC_MESSAGES);
    if (translation == msg_ctxt_id)
        return msgid;
    else
        return translation;
}