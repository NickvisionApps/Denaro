/* This script is modified from example scripts at https://libharu.sourceforge.net/examples.html
 * This script is used to generate a portrait A4 report for https://github.com/nlogozzo/NickvisionMoney
 * 
 * Copyright (c) 1999-2006 Takeshi Kanno <takeshi_kanno@est.hi-ho.ne.jp>
 *
 * Permission to use, copy, modify, distribute and sell this software
 * and its documentation for any purpose is hereby granted without fee,
 * provided that the above copyright notice appear in all copies and
 * that both that copyright notice and this permission notice appear
 * in supporting documentation.
 * It is provided "as is" without express or implied warranty.
 *
 */

#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <setjmp.h>
#include "hpdf.h"

jmp_buf env;

#ifdef HPDF_DLL
void  __stdcall
#else
void
#endif

error_handler (HPDF_STATUS   error_no,
               HPDF_STATUS   detail_no,
               void         *user_data)
{
    printf ("ERROR: error_no=%04X, detail_no=%u\n", (HPDF_UINT)error_no,
                (HPDF_UINT)detail_no);
    longjmp(env, 1);
}

void
draw_line  (HPDF_Page    page,
            const char  *label)
{
    HPDF_Page_MoveTo (page, 56.5, 729.3); 
    HPDF_Page_LineTo (page, 538.8, 729.3); 
    HPDF_Page_Stroke (page);
}

int main (int argc, char **argv)
{
    const char *page_title = "REPORT"; /* Sets page_title */
    HPDF_Doc  pdf;
    char fname[256];
    HPDF_Page page; 
    HPDF_Image image;
    HPDF_Font font_Liberation_Mono_Regular;
    HPDF_Font using_font;

    strcpy (fname, argv[0]);
    strcat (fname, ".pdf");

    pdf = HPDF_New (error_handler, NULL);
    if (!pdf) {
        printf ("error: cannot create PdfDoc object\n");
        return 1;
    }

    if (setjmp(env)) {
        HPDF_Free (pdf);
        return 1;
    }

    /* Add a new page object. */
    page = HPDF_AddPage (pdf);
    /* Set specific page size */
    /* [Note] Removing this line would change the size of the pdf. See https://github.com/libharu/libharu/wiki/Graphics */
    HPDF_Page_SetSize(page, HPDF_PAGE_SIZE_A4, HPDF_PAGE_PORTRAIT);
    
    /* I am trying generate objects from top to bottom, left to right. */
    
    /* Load icon.png */
    /* [Note] The image and this script needs to be in the same directory. */
    #ifndef __WIN32__
    image = HPDF_LoadPngImageFromFile (pdf, "icon.png");
    #else
    image = HPDF_LoadPngImageFromFile (pdf, "icon.png");
    #endif
    /* Draw image to the canvas. */
    /* [Note] Changing the size of the image (the last two numbers) will change the coordinates of the image. */
    HPDF_Page_DrawImage (page, image, 485.3, 731.9, 45.5, 45.5);
    
    /* Try to use specified font */
    /* [Improvement] Get LiberationMono-Regular.ttf file path instead of using a fixed path. This will be incompatible with other distro (Fedora default path here). Needs a message to tell users if the font is not found, adding a "fallback" font would also be nice. */
    font_Liberation_Mono_Regular = HPDF_LoadTTFontFromFile (pdf, "/usr/share/fonts/liberation-mono/LiberationMono-Regular.ttf", HPDF_TRUE);
    using_font = HPDF_GetFont (pdf, font_Liberation_Mono_Regular, NULL);
    
    /* Write page_title */
    /* [Note] Text is aligned to the left. It is safe to change page_title without changing the coordinates. */
    HPDF_Page_SetFontAndSize (page, using_font, 20);
    HPDF_Page_BeginText (page);
    HPDF_Page_TextOut (page, 61, 737.5, page_title);
    HPDF_Page_EndText (page);
    
    /* Draw a Horizontal Rule */
    /* [Note} Changing the width of the rule will change the coordinates of the rule. */
    HPDF_Page_SetLineWidth (page, 3.0);
    HPDF_Page_SetLineCap (page, HPDF_ROUND_END); /* [Note] PDF_BUTT_END can be used to get a regular rectangle, instead of rounded edges ones. */
    draw_line (page, NULL);
    
    /* Save changes to file */
    HPDF_SaveToFile (pdf, fname);

    /* clean up */
    HPDF_Free (pdf);

    return 0;
}
